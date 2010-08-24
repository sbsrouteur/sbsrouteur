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
                           New FrameworkPropertyMetadata(0.0, AddressOf OnValueChanged))



    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Property Format() As String
        Get
            Return _Format
        End Get
        Set(ByVal value As String)
            _Format = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Format"))
            UpdateNumber()
        End Set
    End Property

    Public Property Digits() As ObservableCollection(Of Digit)
        Get
            Return _digits
        End Get
        Set(ByVal value As ObservableCollection(Of Digit))
            _digits = value
        End Set
    End Property

    Private Shared Sub OnValueChanged(ByVal O As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        CType(O, NumberDisplay).SetValue(ValueProperty, e.NewValue)
        CType(O, NumberDisplay).UpdateNumber()

    End Sub

    Private Sub UpdateNumber()

        Dim str As String = Value.ToString(Format)

        Digits.Clear()
        For Each C As Char In str

            If C >= "0"c And C <= "9" Then
                Digits.Add(New Digit() With {.Digit = AscW(C) - AscW("0"c), .Dot = False})
            ElseIf Digits.Count > 0 Then
                Digits(Digits.Count - 1).Dot = True
            End If
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Digits"))

    End Sub



End Class
