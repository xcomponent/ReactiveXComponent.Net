@rem EXPORT INTERFACE BATCH FILE
if [%1]==[] (
	mkdir "../exportedInterfaces"
	XComponent.XCTools.exe --compilationmode=Debug --exportInterface -env=Dev -output="../exportedInterfaces" -project="../HelloWorldV5_Model.xcml"
) else (
	XComponent.XCTools.exe --compilationmode=Debug --exportInterface -env=Dev -output=%1 -project="../HelloWorldV5_Model.xcml"
)