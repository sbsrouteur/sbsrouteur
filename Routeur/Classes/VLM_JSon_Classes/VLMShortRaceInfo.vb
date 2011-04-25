Public Class VLMShortRaceInfo

    Public Property idraces As String
    Public Property racename As String
    Public Property started As Integer
    Public Property deptime As Long
    Public Property startlong As Double
    Public Property startlat As Double
    Public Property boattype As String
    Public Property closetime As Long
    Public Property racetype As Long
    Public Property firstpcttime As Double
    Public Property depend_on As String
    Public Property qualifying_races As String
    Public Property idchallenge As String
    Public Property coastpenalty As Long
    Public Property bobegin As Long
    Public Property boend As Long
    Public Property maxboats As Long
    Public Property theme As String
    Public Property vacfreq As Long
    Public Property updated As String

    Public ReadOnly Property StartDate As DateTime
        Get
            'Return EpochToDate()
        End Get
    End Property
End Class
