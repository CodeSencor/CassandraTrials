# CassandraTrials

## 160288

A flight management distributed database technology demonstrator

# Architecture

The solution consists of three primary components:
 - **Cassandra DB cluster** which serves as a distributed database system, consists of three linked nodes on a single virtual rack
 - **ApplicationHost server** which serves as an intermediary between ClientCli and the cluster for executing commands
 - **ClientCli client** which serves as an actual user-exposed interface for executing commands on the database

# Database schema

The database has two tables:

Flight (flight_id, departure, destination, flight_no, origin, seats) - stores flight data

Reservations (flight_id, seat_no, passenger_name, reservation_id) - stores reservation information for each seat on each flight

# Execution flow

Server and client use named pipes for communication with each other. Two pipe types are used in the project:
 - cas_cmd_channel - this pipe is used for command invocation, it is also the first pipe used during the command execution
 - cas_cmd_response_channel - this pipe is used for sending the response content back to client after executing the command, it's the second pipe used during the command execution

Once the connection between the client and the server is established, the client serializes the command with arguments to a JSON data object and sends it via the StreamWriter to the server. Once received the object is unpacked and added into an internal channel queue. This is later dispatched by a dedicated service for launching commands. The ClientCli is launched once per command, while the server is persistent.

# Problems encountered

The architecture picked for the project is esoteric. A lot had to be read and understood and reread and reunderstood to create this system, which could very possibly be done by using a standalone console application.

Most problems encountered were design related rather than stemming from the bugs or the db logic. The project spans eight different custom services, making it time consuming to say the least.

The Datasax Cassandra driver is well documented and could be easily understood, however coming from purely SQL languages took a minute to adjust to the differences in CQL syntax.

Getting the basic commands to run was fairly easy, but the stress tests required more attention to make operational. While the cluster itself did not require any adjustments, the internal dispatch logic needed for the last two tests took longer to design.

Initially the cluster nodes were spontaneously shutting down, however by adjusting the memory levels they began working correctly.

# How to run

Docker compose was not used for this project. All containers were created from `docker run`. 

## Stage 1 - Docker setup.

Pull the latest Cassandra image from the Docker hub: `docker pull cassandra`

Create a bridge network that all nodes will use: `docker network create -d bridge cas-net`

Create cluster nodes: 

`docker run --name cas1 --network cas-net -d -e MAX_HEAP_SIZE=1G -e HEAP_NEWSIZE=256M cassandra`

`docker run --name cas2 --network cas-net -d -e CASSANDRA_SEEDS=cas1 -e MAX_HEAP_SIZE=1G -e HEAP_NEWSIZE=256M cassandra`

`docker run --name cas3 --network cas-net -d -e CASSANDRA_SEEDS=cas1,cas2 -e MAX_HEAP_SIZE=1G -e HEAP_NEWSIZE=256M cassandra`

Download the `Dockerfile` from the repository and build it locally: `docker build -t evaluator .`

Create an interactive node on the same bridge network using the `Dockerfile` image: `docker run -it --name evaluator --network cas-net -d evaluator`

The interactive node mounts a volume, serving as a shared space between the host and the container. For convenience I recommend creating a symlink to it in a more suitable location.

## Step 1.5 - adding database schema.

Execute `docker exec -it cas1 cqlsh`

Create keyspace: `CREATE KEYSPACE IF NOT EXISTS flight_mgmt WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3};`

Activate keyspace: `USE flight_mgmt;`

Create `Flight` table:
```
CREATE TABLE flight (
  flight_id   uuid PRIMARY KEY,
  flight_no   text,
  origin      text,
  destination text,
  departure   timestamp,
  seats       int
);
```

Create `Reservation` table:
```
CREATE TABLE reservation (
  reservation_id uuid PRIMARY KEY,
  flight_id      uuid,
  passenger_name text,
  seat_no        text
);
```

Insert initial data to the `Flight` table:
```
INSERT INTO flight (flight_id, flight_no, origin, destination, departure, seats)
VALUES (11111111-1111-1111-1111-111111111111, 'LO123', 'WAW', 'LHR', '2026-07-01 08:00:00+0000', 180);

INSERT INTO flight (flight_id, flight_no, origin, destination, departure, seats)
VALUES (22222222-2222-2222-2222-222222222222, 'LO456', 'WAW', 'CDG', '2026-07-01 11:30:00+0000', 150);

INSERT INTO flight (flight_id, flight_no, origin, destination, departure, seats)
VALUES (33333333-3333-3333-3333-333333333333, 'FR789', 'KRK', 'STN', '2026-07-02 06:15:00+0000', 200);
```

## Step 2 - compilation.

Install .NET 10.0 SDK on your machine

Clone the repository. I recommend doing this in the interactive node folder, since this will dodge the hiccup of copying compiled apps back to the container

In the repository main folder compile the solution using `dotnet publish -c Release -r linux-musl-x64 --self-contained true`

## Step 3 - running the system.

Execute `docker exec -it evaluator /usr/bin/fish`. You will need one terminal for the server and one for the client, do it twice.

If you cloned the repository into the volume directly, the two files of interest lie on these absolute paths:

`/evaluator/CassandraTrials/CassandraTrials/bin/Release/net10.0/linux-musl-x64/publish`

`/evaluator/CassandraTrials/ClientCli/bin/Release/net10.0/linux-musl-x64/publish`

The launch procedure is as follows:

In one terminal launch the server through `./CassandraTrials`

In the second terminal launch the client through `./ClientCli <command> <args>`

Commands are invoked by their name. Check [this](https://github.com/CodeSencor/CassandraTrials/tree/master/CassandraTrials/Commands) directory to learn more.

Stress tests are commands themselves. They do not take any arguments.
