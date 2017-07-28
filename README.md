# DMF

## Project Description
DMF, or Data Model Format, is a simple data processing extension for SSRS on SQL Server Data Tools - Business Intelligence.
The main idea of this extension is to provide dummy/mock data to SSDT report designer based on the specification by report author, without having to connect to the actual datasource (i.e. SQL Server Data Source).

## Changeset(s)
2013-12-26
  - Added enumeration feature to synchronize group reference and group field(s) value
  - Redefined <Group /> element on DataModelFormat.xsd
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
  - Removed <Index /> element
  - Added @Index and @Ref attributes
  - Added support for group fields reference, i.e. field(s) can now be linked with field group to simulate single row data

2013-12-17
  - Fixed field name issue
  - Added support for <Value /> element, as an alternative to <DataField /> element to specify field name and data type
