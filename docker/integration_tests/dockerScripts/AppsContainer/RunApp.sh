echo "Updating XCApi bus info"
if [ -n "${XCAPIFILE:-}" ]
then
	sed -i -e 's/daniel//g' -e 's/5672/'$PORT'/g' -e 's/localrabbitmq/RabbitMq/g' -e 's/RABBIT_MQ/'$BUSTYPE'/g' -e 's/127.0.0.1/'$HOST'/g' $XCAPIFILE
fi

chmod +x wait_for_it.sh

echo "App launched"
sleep 35
cd ./Debug/ && bash ./../wait_for_it.sh rabbitmq:5672 -- dotnet $APP
