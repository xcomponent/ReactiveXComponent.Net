@rem GENERATE PACKAGE BATCH FILE
if [%1]==[] (
	mkdir "../generatedPackage"
	XComponent.XCTools.exe --compilationmode=Debug --generatePackage -env=Dev -output="../generatedPackage" -project="../HelloWorldV5_Model.xcml"
) else (
	XComponent.XCTools.exe --compilationmode=Debug --generatePackage -env=Dev -output=%1 -project="../HelloWorldV5_Model.xcml"
)