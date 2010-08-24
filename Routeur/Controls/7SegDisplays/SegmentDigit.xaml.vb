Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes

Partial Public Class SegmentDigit




    Public Shared ReadOnly ShowDotProperty As DependencyProperty = _
                           DependencyProperty.Register("ShowDot", _
                           GetType(Boolean), GetType(SegmentDigit), _
                           New FrameworkPropertyMetadata(False))




    Public Shared ReadOnly DigValueProperty As DependencyProperty = _
                           DependencyProperty.Register("DigValue", _
                           GetType(Integer), GetType(SegmentDigit), _
                           New FrameworkPropertyMetadata(0))



    Public Property DigValue() As Integer
        Get
            Return CInt(GetValue(DigValueProperty))
        End Get

        Set(ByVal value As Integer)
            SetValue(DigValueProperty, value)
        End Set
    End Property



    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Property ShowDot() As Boolean
        Get
            Return CBool(GetValue(ShowDotProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(ShowDotProperty, value)
        End Set
    End Property

End Class

Public Class SegmentVisiblityConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim ret As Visibility = Visibility.Hidden
        Dim IVal As Integer = CInt(value)

        Select Case CInt(parameter)
            Case 0

                If Not IVal = 1 AndAlso Not IVal = 4 Then
                    ret = Visibility.Visible
                End If

            Case 1

                If Not IVal = 1 AndAlso Not IVal = 7 AndAlso Not IVal = 0 Then
                    ret = Visibility.Visible
                End If

            Case 2

                If Not IVal = 1 AndAlso Not IVal = 4 AndAlso Not IVal = 7 Then
                    ret = Visibility.Visible
                End If

            Case 3
                If Not IVal = 1 AndAlso Not IVal = 2 AndAlso Not IVal = 3 AndAlso Not IVal = 7 Then
                    ret = Visibility.Visible
                End If

            Case 4
                If Not IVal = 5 AndAlso Not IVal = 6 Then
                    ret = Visibility.Visible
                End If

            Case 5
                If Not IVal = 1 AndAlso Not IVal = 3 AndAlso Not IVal = 4 AndAlso Not IVal = 5 AndAlso Not IVal = 7 AndAlso Not IVal = 9 Then
                    ret = Visibility.Visible
                End If

            Case 6
                If Not IVal = 2 Then
                    ret = Visibility.Visible
                End If

        End Select

        Return ret
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function

End Class
