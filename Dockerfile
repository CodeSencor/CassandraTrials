FROM alpine:latest

RUN apk update
RUN apk add dotnet10-runtime fish
RUN adduser -D -s /usr/bin/fish evaluator
RUN mkdir /evaluator
RUN chown evaluator:evaluator /evaluator
RUN chmod g+s /evaluator
VOLUME /evaluator
WORKDIR /evaluator
USER evaluator
CMD [ "/usr/bin/fish" ]
