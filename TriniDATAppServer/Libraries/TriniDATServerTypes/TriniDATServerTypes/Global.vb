Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

'Is used to point internally to JServiceLauncher.getCoreType
Public Delegate Function object_creator(ByVal class_name As String, ByVal construct_params() As Object) As Object

Public Enum TRINIDAT_SERVERMODE
    MODE_LIVE = 1
    MODE_DEV = 2
End Enum

Public Enum TRINIDAT_SERVER_LICENSE

    T_LICENSE_FREE = 0
    T_LICENSE_CORPORATE = 1
    T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM = 2
    T_LICENSE_CORPORATE_UNIVERSITY_LARGE = 3
    T_LICENSE_LIBERATED = 4

End Enum
