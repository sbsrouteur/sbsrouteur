Public Class TextToColorConverter

    Implements IValueConverter
    Private Const VALUE_OK_COLOR As Long = &HFF7AFFAE
    Private Const VALUE_NOK_COLOR As Long = &HFFFF7A7A

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is String And targetType Is GetType(Double) Then

            Dim v As Double
            If Double.TryParse(CStr(value), v) Then
                'Convertback and compare
                If v.ToString = CStr(value).Trim Then
                    Return System.Drawing.Color.FromArgb(VALUE_OK_COLOR)
                Else
                    Return System.Drawing.Color.FromArgb(VALUE_NOK_COLOR)
                End If
            Else
                'Does not parse as double
                Return System.Drawing.Color.FromArgb(VALUE_NOK_COLOR)
            End If
        End If

        Return Nothing

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return value

    End Function
End Class
