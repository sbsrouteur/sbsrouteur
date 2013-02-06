Public Class DateConverter

    Public Shared Function GetDate(ByVal xmldate As String) As DateTime

        Dim retdate As DateTime

        DateTime.TryParse(xmldate, retdate)
        retdate = New DateTime(retdate.Ticks, DateTimeKind.Utc)

        Return retDate
    End Function

End Class
