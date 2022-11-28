FROM xcomponent/mono:1.0.0
USER root
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y apt-transport-https
RUN apt-get update && apt-get install -y \
	iputils-ping \
	telnet \
	net-tools \
	dos2unix \
	unzip \
	nodejs \
	&& rm -rf /var/lib/apt/lists/* /tmp/* 
COPY XCRuntime.zip ./
RUN rm -rf XCRuntime
RUN unzip /XCRuntime.zip -d XCRuntime

COPY wait_for_it.sh ./
COPY HelloWorldV5XCassemblies.zip ./
COPY launchRuntime.sh ./

EXPOSE 5400
EXPOSE 9676

ENV HOST=rabbitmq 
ENV PORT=5672
ENV BUSTYPE=RABBIT_MQ
ENV XCPROPERTIES=
CMD dos2unix wait_for_it.sh && dos2unix ./launchRuntime.sh && bash ./launchRuntime.sh