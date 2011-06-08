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

    Public ReadOnly Property IsStarted As Boolean
        Get
            Return started = 1
        End Get
    End Property

    Public ReadOnly Property CanEngage As Boolean
        Get
            Dim StartDate As DateTime
            Dim EndDate As DateTime

            Dim Cvt As New EpochToUTCDateConverter

            StartDate = CDate(Cvt.Convert(deptime, GetType(DateTime), Nothing, System.Globalization.CultureInfo.CurrentCulture)).ToLocalTime
            EndDate = CDate(Cvt.Convert(closetime, GetType(DateTime), Nothing, System.Globalization.CultureInfo.CurrentCulture)).ToLocalTime

            Return Now >= StartDate AndAlso Now < EndDate


        End Get
    End Property

End Class
