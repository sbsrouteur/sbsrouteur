Imports System.ComponentModel
Imports agsXMPP.protocol.client
Imports agsXMPP


Public Class JidExtension
    Inherits Jid
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged



    Public Sub New(jid As String)
        MyBase.New(jid)
    End Sub

    Public Sub New(J As Jid)
        MyBase.New(J.ToString)
    End Sub


    Private _Presence As PresenceType = PresenceType.unavailable

    Public Property Presence As PresenceType
        Get
            Return _Presence
        End Get
        Set(value As PresenceType)
            _Presence = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Presence"))
        End Set
    End Property

    Public ReadOnly Property IsPresent As Boolean
        Get
            Return _Presence = PresenceType.available
        End Get
    End Property


End Class
