Imports System.ComponentModel

Public Class RegistryPlayerInfo
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Nick As String
    Private _BoatNum As Integer
    Private _IsMine As Boolean
    Private _Password As String
    Private _IsLoaded As Boolean = False

    Public Sub New(ByVal Id As Integer)
        _BoatNum = Id
    End Sub

    Public Property BoatNum() As Integer
        Get
            If Not _isloaded Then
                Load()
            End If
            Return _BoatNum
        End Get
        Set(ByVal value As Integer)

            If _BoatNum <> value Then
                _BoatNum = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatNum"))
                SaveUserInfo(Me)
            End If
        End Set
    End Property

    Public ReadOnly Property Playerinfo() As clsPlayerInfo
        Get
            Dim Ret As New clsPlayerInfo
            With Ret
                .Nick = Nick
                .Password = Password
                .NumBoat = BoatNum
            End With
            Return Ret
        End Get
    End Property

    Public Property IsMine() As Boolean
        Get
            If Not _IsLoaded Then
                Load()
            End If
            Return _IsMine
        End Get
        Set(ByVal value As Boolean)
            If _IsMine <> value Then
                _IsMine = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsMine"))
                SaveUserInfo(Me)
            End If
        End Set
    End Property

    Public Property Nick() As String
        Get
            If Not _IsLoaded Then
                Load()
            End If
            Return _Nick
        End Get
        Set(ByVal value As String)

            If _Nick <> value Then
                _Nick = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Nick"))
                SaveUserInfo(Me)
            End If
        End Set
    End Property

    Public Property Password() As String
        Get
            If Not _IsLoaded Then
                Load()
            End If
            Return _Password
        End Get
        Set(ByVal value As String)

            If _Password <> value Then
                _Password = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Password"))
                SaveUserInfo(Me)
            End If
        End Set
    End Property

    Private Sub Load()

        LoadUserInfo(Me, _BoatNum)
        _IsLoaded = True

    End Sub




End Class
