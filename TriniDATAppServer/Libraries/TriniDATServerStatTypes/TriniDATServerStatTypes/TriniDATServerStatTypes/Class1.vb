Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 


Public Enum STAT_EXECUTION_CONTEXT
    CONTEXT_UNKNOWN = 0
    CONTEXT_ACTIVEWEBSERVICE_LEVEL = 1 'inside a JClass
    CONTEXT_MAPPING_POINT_LEVEL = 2
    CONTEXT_SOCKET_LEVEL = 3
    CONTEXT_STATS = 4
End Enum