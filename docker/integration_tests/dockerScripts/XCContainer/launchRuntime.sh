#!/usr/bin/env bash
set -o errexit
set -o nounset

echo "Extracting XCR file"
rm -rf xcassemblies
unzip /$XCASSEMBLIESZIP -d xcassemblies

echo "Updating xcproperties bus info"
if [ -n "${XCPROPERTIES:-}" ]
then	
	cd ./XCRuntime/
	ls -lrt
	sed -i -e 's/RABBIT_MQ/'$BUSTYPE'/g' -e 's/5672/'$PORT'/g' -e 's/rabbitmq/'$HOST'/g' $XCPROPERTIES
	cd ..
fi

chmod +x wait_for_it.sh
 
if [ -n "${TRIGGEREDMETHODSLAUNCHER:-}" ]
then
	echo "Launching XCruntime as a daemon..."
	cd ./XCRuntime/
	bash ./../wait_for_it.sh rabbitmq:5672 -- mono xcruntime.exe $XCRFILE ${XCPROPERTIES:-} ${EXTRA_ARG:-} &

	cd ..
	rm -rf TriggeredMethods
	unzip /$TRIGGEREDMETHODSZIP -d TriggeredMethods

	echo "Launching triggered methods..."
	cd ./TriggeredMethods/
	dos2unix $TRIGGEREDMETHODSLAUNCHER
	chmod +x $TRIGGEREDMETHODSLAUNCHER
	npm install
	bash ./../wait_for_it.sh 127.0.0.1:9676 -- ./$TRIGGEREDMETHODSLAUNCHER 
else
	echo "Launching XCruntime..."
	cd ./XCRuntime/
	bash ./../wait_for_it.sh rabbitmq:5672 -- mono xcruntime.exe $XCRFILE ${XCPROPERTIES:-} ${EXTRA_ARG:-}
fi



