# DMF

## Project Description
DMF, or Data Model Format, is a simple data processing extension for SSRS on SQL Server Data Tools - Business Intelligence.
The main idea of this extension is to provide dummy/mock data to SSDT report designer based on the specification by report author, without having to connect to the actual datasource (i.e. SQL Server Data Source).

## How to Deploy the Extension
1. Build the solution (The solution published on CodePlex is created using Visual Studio 2013, however the project itself is developed on all Visual Studio 2010 Professional Edition, Visual Studio 2012 Express for Windows Desktop, and Visual Studio 2013 Ultimate Edition)
2. Copy `TOJO.ReportServerExtension.dll` assembly to SSDT-BI's PrivateAssembly folder. Typically to: `C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\PrivateAssemblies`
3. Add `RSReportDesigner.snippet.xml` to `RSReportDesigner.config` file on SSDT-BI's PrivateAssembly folder. Configure `<SpecificationFolder />` inner text to wherever you want to put your specification files  
   The content of the snippet illustrates the hierarchy of the element. You only need to copy-paste `<CodeGroup />` element for RSPreviewPolicy.config, and `<Extension />` element for RSReportDesigner.config
4. Add `RSPreviewPolicy.snippet.xml` to `RSPreviewPolicy.config` file on SSDT-BI's PrivateAssembly folder. Modify the path of assembly configured on Url attribute of `<IMembershipCondition />` element if needed
5. Restart SSDT-BI if currently opened

## How to Specify DMF
1. Create a new file with .dmf extension (e.g. `DataSet1.dmf`) on the specification folder (configured on `RSReportDesigner.config` earlier)
2. Specify the format of the data that you intend to acquire based on `DataModelFormat.xsd` schema. Optionally, you could copy and paste `<Fields />` element of any existing dataset on your .rdl file (schema version 2010, i.e. SQL Server 2008 R2 or above) and add the following attribute to root `<Fields />` element of your specification: `xmlns:rd="http://schemas.microsoft.com/SQLServer/reporting/reportdesigner"`

## How to Consume the Data on SSRS Report
1. Create a new report file
2. Create a new datasource, set the type to "TOJO Data Source". Connection string and credentials are not required
3. Create a new dataset, set the data source to the one you previously created
4. Currently, this extension only supports query with the type of "Text". Therefore, set query type to: "Text"
5. Set "--Name: {your DMF file name without .dmf extension}" as the query. Using the example name of DMF file earlier, the query would be something like: "--Name: DataSet1" (without the surrounding quotes)
  You can specify SQL query as you would on any dataset using SQL Server data source. This extension reads the first line prefixed with `"--Name: "` as its query. Any other queries are ignored. You can prepend the DMF query to your existing query (SQL Server will treat it as comment). Don't forget to change the datasource type to SQL Server before publishing the report
6. Click "Refresh Fields" button 
If everything goes well, the specified field/column will be auto-generated on the fields tab of dataset properties 
Design and preview the behaviour of your report layout

## Changeset(s)
2013-12-26
- Added enumeration feature to synchronize group reference and group field(s) value
- Redefined `<Group />` element on DataModelFormat.xsd
- Added @Enum attribute
- Fixed default and nullable value issue for group reference

2013-12-25
- Removed backward compatibility support for DataModelFormat.xsd revised version from the library (additional projects are added as a separate compatibility support)
- Enabled folder hierarchy on dataset query
  Report author can now specifiy dataset query using either the following remark:
  ```sql
  --Name: DMFSpecificationFileNameWithoutFileExtension
  --Name: DMF Specification File Name Without File Extension
  --Name: "Folder Name\DMF Specification File Name Without File Extension"
  ```
  Please note that file or folder path are relative to specification folder configured on RSReportDesigner.config.
  In addition, report author can also specify folder hierarchy using forward-slash; as the following query remark illustrates:
  ```sql
  --Name: "Folder Name/DMF Specification File Name Without File Extension"
  ```
  It was unexpectedly beneficial.

2013-12-23
- Redefined `<Group />` element on DataModelFormat.xsd
- Removed `<Index />` element
- Added @Index and @Ref attributes
- Added support for group fields reference, i.e. field(s) can now be linked with field group to simulate single row data

2013-12-17
- Fixed field name issue
- Added support for `<Value />` element, as an alternative to `<DataField />` element to specify field name and data type
