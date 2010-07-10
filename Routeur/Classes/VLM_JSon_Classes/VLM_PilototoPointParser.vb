Imports Routeur.RoutePointView

Module VLM_PilototoPointParser

    Public Const FLD_ID As Integer = 0
    Public Const FLD_DATEVALUE As Integer = 1
    Public Const FLD_ORDERTYPE As Integer = 2
    Public Const FLD_ORDERVALUE As Integer = 3

    Public Const MIN_PILOTOTO_FIED_COUNT As Integer = 5



    Public Function GetBearingAngleValue(ByVal OrderValue As String, ByRef RetValue As Double) As Boolean
        Return Double.TryParse(OrderValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetValue)
    End Function

    Public Function GetOrderDate(ByVal Datestring As String, ByRef RetValue As DateTime) As Boolean
        Dim LongValue As Long
        If Long.TryParse(Datestring, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, LongValue) Then
            RetValue = New Date(1970, 1, 1).AddSeconds(LongValue).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function GetOrderID(ByVal OrderString As String, ByRef RetID As Integer) As Boolean

        Dim IntValue As Integer
        If Integer.TryParse(OrderString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, IntValue) Then
            RetID = CType(IntValue, EnumRouteMode)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function GetOrderType(ByVal OrderString As String, ByRef RetValue As EnumRouteMode) As Boolean

        Dim IntValue As Integer
        If Integer.TryParse(OrderString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, IntValue) Then
            RetValue = CType(IntValue, EnumRouteMode)
            Return True
        Else
            Return False
        End If

    End Function

    Public Function OrderIsPending(ByVal OrderString As String) As Boolean

        Return OrderString.ToLowerInvariant.Contains("pending")

    End Function

End Module
