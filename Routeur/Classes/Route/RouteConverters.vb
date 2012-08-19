Public Class EnumRouteModeConverter
    Implements IValueConverter


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If TypeOf value Is RoutePointView.EnumRouteMode Then

            If targetType Is GetType(Double) Then
                Return CDbl(value)
            Else
                'Return CType(value, RoutePointViewBase.EnumRouteMode).ToString()
                Return [Enum].GetName(GetType(RoutePointView.EnumRouteMode), value)
            End If
        Else
            Return value
        End If

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        If TypeOf value Is String AndAlso targetType Is GetType(RoutePointView.EnumRouteMode) Then
            Dim Names() As String = [Enum].GetNames(GetType(RoutePointView.EnumRouteMode))
            Dim Values As Array = [Enum].GetValues(GetType(RoutePointView.EnumRouteMode))
            Dim i As Integer

            For i = 0 To Names.Count - 1
                If Names(i) = CStr(value) Then
                    Return Values.GetValue(i)
                End If
            Next
            Return value
        Else
            Return value
        End If
    End Function

End Class

Public Class EnumRoutePendingBrushConverter

    Implements IValueConverter

    Dim PendingBrush As New Windows.Media.SolidColorBrush(Windows.Media.Colors.LightGreen)
    Dim DoneBrush As New Windows.Media.SolidColorBrush(Windows.Media.Colors.LightPink)


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        If CBool(value) Then
            Return PendingBrush
        Else
            Return DoneBrush
        End If

    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack

        Return value

    End Function
End Class