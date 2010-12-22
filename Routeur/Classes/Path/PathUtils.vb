Module PathUtils

    Public Function GetCoordsString(ByVal X As Double, ByVal Y As Double) As String
        Return X.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," & Y.ToString(System.Globalization.CultureInfo.InvariantCulture)
    End Function


End Module
