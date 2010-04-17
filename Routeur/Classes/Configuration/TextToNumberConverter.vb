Public Class TextToNumberConverter

    Implements IValueConverter

    'post V0.16
    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim i As Integer = 0

        If TypeOf value Is Double AndAlso targetType Is GetType(String) Then
            Return value.ToString
        End If

        Return value
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function

End Class
