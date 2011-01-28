Public Class GSHHS_StartInfo

    Public Delegate Sub LoadCompleteDelegate()

    Public StartPath As String
    Public PolyGons As LinkedList(Of Polygon)
    Public ProgressWindows As MapProgressContext
    Public CompleteCallBack As LoadCompleteDelegate
    Public NoExclusionZone As Boolean
    
End Class
