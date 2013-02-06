﻿Imports System.ComponentModel

Public Class Digit

    Implements INotifyPropertyChanged

    Private _Dot As Boolean = False
    Private _Digit As Char = "0"c


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Property Digit() As Char
        Get
            Return _Digit
        End Get
        Set(ByVal value As Char)
            _Digit = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Digit"))
        End Set
    End Property

    Public Property Dot() As Boolean
        Get
            Return _Dot
        End Get
        Set(ByVal value As Boolean)
            _Dot = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Dot"))
        End Set
    End Property
End Class
