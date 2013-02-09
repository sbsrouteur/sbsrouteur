'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

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
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Partial Public Class NumberDisplay
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _digits As New ObservableCollection(Of Digit)
    Private _Format As String




    Public Property StringFormat() As String
        Get
            Return CStr(GetValue(StringFormatProperty))
        End Get

        Set(ByVal value As String)
            SetValue(StringFormatProperty, value)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("StringFormat"))
        End Set
    End Property

    Public Shared ReadOnly StringFormatProperty As DependencyProperty = _
                           DependencyProperty.Register("StringFormat", _
                           GetType(String), GetType(NumberDisplay), _
                           New FrameworkPropertyMetadata(Nothing, AddressOf OnValueFormatChanged))




    Public Property Value() As Double
        Get
            Return CDbl(GetValue(ValueProperty))
        End Get

        Set(ByVal Newvalue As Double)
            SetValue(ValueProperty, Newvalue)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Value"))
        End Set
    End Property

    Public Shared ReadOnly ValueProperty As DependencyProperty = _
                           DependencyProperty.Register("Value", _
                           GetType(Double), GetType(NumberDisplay), _
                           New FrameworkPropertyMetadata(0.0, AddressOf OnValueFormatChanged))



    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Property Digits() As ObservableCollection(Of Digit)
        Get
            Return _digits
        End Get
        Set(ByVal value As ObservableCollection(Of Digit))
            _digits = value
        End Set
    End Property


    Private Shared Sub OnValueFormatChanged(ByVal O As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        CType(O, NumberDisplay).UpdateNumber()

    End Sub

    Private Sub UpdateNumber()

        Dim str As String

        If StringFormat = "lon" Then
            Dim cv As New CoordsConverter
            str = CStr(cv.Convert(Value, GetType(String), 1, System.Threading.Thread.CurrentThread.CurrentUICulture))
            If Value >= 0 Then
                str &= " E"
            Else
                str &= " W"
            End If
            
        ElseIf StringFormat = "lat" Then
            Dim cv As New CoordsConverter
            str = CStr(cv.Convert(Value, GetType(String), 1, System.Threading.Thread.CurrentThread.CurrentUICulture))
            If Value >= 0 Then
                str &= " N"
            Else
                str &= " S"
            End If

        ElseIf StringFormat = "nav" Then
            If Value = 3 Then
                str = "Ortho"
            ElseIf Value = 4 Then
                str = "VMG"
            ElseIf Value = 5 Then
                str = "vbvmg"
            Else
                str = ""
            End If
        Else
            str = Value.ToString(StringFormat)
        End If
        str = str.ToUpperInvariant

        Digits.Clear()
        For Each C As Char In str

            If (C >= "0"c And C <= "9") OrElse (C >= "A"c And C <= "Z"c) OrElse C = " "c Then
                Digits.Add(New Digit() With {.Digit = C, .Dot = False})
            ElseIf C = "-"c OrElse C = "°"c Then
                Digits.Add(New Digit() With {.Digit = C, .Dot = False})
            ElseIf Digits.Count > 0 Then
                Digits(Digits.Count - 1).Dot = True
            End If

        Next


    End Sub



End Class
