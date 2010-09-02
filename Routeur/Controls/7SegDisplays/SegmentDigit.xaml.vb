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
                           GetType(Char), GetType(SegmentDigit), _
                           New FrameworkPropertyMetadata("0"c))



    Public Property DigValue() As Char
        Get
            Return CChar(GetValue(DigValueProperty))
        End Get

        Set(ByVal value As Char)
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

    Private Enum MapLookup As Byte
        l0 = 0
        l1 = 1
        l2 = 2
        l3 = 3
        l4 = 4
        l5 = 5
        l6 = 6
        l7 = 7
        l8 = 8
        l9 = 9
        lminus = 10
        lA = 11
        lZ = lA + 25
        lSpace = 37
        lDeg = 38

    End Enum

    Private Shared _Maps() As Integer = New Integer() {&H7D, &H50, &H37, &H57, &H5A, &H4F, &H6F, &H51, &H7F, &H5F, &H2, _
                                                       &H7B, &H6E, &H2D, &HAA, &H2F, &H27, &H4C5, &H7A, &H50, &H53, &H0, &H84, &H79, &H378, _
                                                       &H7D, &H37, &H0, &H293, &H4F, &H0, &H7C, &H0, &H87C, &H0, &H0, &H0, _
                                                       &H0, &H1000}


    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim ret As Visibility = Visibility.Hidden
        Dim CVal As Char = CChar(value)
        Dim MapIndex As Integer

        If CVal >= "0"c AndAlso CVal <= "9"c Then
            MapIndex = MapLookup.l0 + AscW(CVal) - AscW("0"c)
        ElseIf CVal >= "A"c AndAlso CVal <= "Z"c Then
            MapIndex = MapLookup.lA + AscW(CVal) - AscW("A"c)
        ElseIf CVal = "-"c Then
            MapIndex = MapLookup.lminus
        ElseIf CVal = " " Then
            MapIndex = MapLookup.lSpace
            'ElseIf CVal = "°"c Then
            '    MapIndex = MapLookup.lDeg
        Else
            Return ret
        End If

        If ((1 << CInt(parameter)) And _Maps(MapIndex)) <> 0 Then
            ret = Visibility.Visible
        End If

        'Select Case CInt(parameter)
        'Case 0

        '    If Not CVal = "1"c AndAlso Not CVal = "4"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 1

        '    If Not CVal = "1"c AndAlso Not CVal = "7"c AndAlso Not CVal = "0"c AndAlso Not CVal = "-"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 2

        '    If Not CVal = "1"c AndAlso Not CVal = "4"c AndAlso Not CVal = "7"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 3
        '    If Not CVal = "1"c AndAlso Not CVal = "2"c AndAlso Not CVal = "3"c AndAlso Not CVal = "7"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 4
        '    If Not CVal = "5"c AndAlso Not CVal = "6"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 5
        '    If Not CVal = "1"c AndAlso Not CVal = "3"c AndAlso Not CVal = "4"c AndAlso Not CVal = "5"c AndAlso Not CVal = "7"c AndAlso Not CVal = "9"c Then
        '        ret = Visibility.Visible
        '    End If

        'Case 6
        '    If Not CVal = "2"c Then
        '        ret = Visibility.Visible
        '    End If

        'End Select

        Return ret
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Return value
    End Function

End Class
