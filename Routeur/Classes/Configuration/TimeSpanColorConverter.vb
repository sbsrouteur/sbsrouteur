Public Class TimeSpanColorConverter
    Implements IValueConverter

    Private Shared BrushOK As New SolidColorBrush(New Color() With {.A = &HFF, .R = &H7A, .G = &HFF, .B = &HAE})
    Private Shared BrushNOK As New SolidColorBrush(New Color() With {.A = &HFF, .R = &HFF, .G = &H7A, .B = &H7A})

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        'Return System.Drawing.Color.Pink

        Dim v As TimeSpan
        If TimeSpan.TryParse(CStr(value), v) Then
            'Convertback and compare
            If v.ToString = CStr(value).Trim Then
                Return BrushOK
            End If
        End If
        'Does not parse as double
        Return BrushNOK

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return value

    End Function

End Class
