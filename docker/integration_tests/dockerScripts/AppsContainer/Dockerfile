FROM microsoft/dotnet:sdk
USER root

RUN apt-get update && apt-get install -y unzip iputils-ping telnet net-tools dos2unix && rm -rf /var/lib/apt/lists/* /tmp/*

COPY wait_for_it.sh ./

ARG APPZIP
COPY $APPZIP ./
RUN rm -rf Debug
RUN unzip $APPZIP -d Debug

ADD RunApp.sh /RunApp.sh

ENV HOST=rabbitmq 
ENV PORT=5672
ENV BUSTYPE=RABBIT_MQ
ENV XCAPIFILE=
CMD dos2unix ./RunApp.sh  && bash ./RunApp.sh 