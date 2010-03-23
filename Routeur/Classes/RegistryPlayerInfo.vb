Imports System.ComponentModel
Imports System.Security.Cryptography
Imports System.IO

Public Class RegistryPlayerInfo
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Nick As String
    Private _BoatNum As Integer
    Private _IsMine As Boolean
    Private _Password As String
    Private _IsLoaded As Boolean = False
    Private _RaceInfo As String = ""
    Private _IsRacing As Boolean = False
    Private _IsPasswordOK As Boolean = False
    Private _RaceNum As Integer

    Public Sub New(ByVal Name As String)
        _Nick = Name
    End Sub

    Public Property BoatNum() As Integer
        Get
            If Not _IsLoaded Then
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

    Public ReadOnly Property IsPasswordOK() As Boolean
        Get
            Return _IsPasswordOK
        End Get
    End Property

    Public ReadOnly Property RaceInfo() As String
        Get
            If Not _IsLoaded Then
                Load()
            End If
            Return _RaceInfo
        End Get

    End Property

    Public ReadOnly Property IsRacing() As Boolean
        Get
            If Not _IsLoaded Then
                Load()
            End If
            Return _IsRacing
        End Get
    End Property

    Public ReadOnly Property IsNotRacing() As Boolean
        Get
            Return Not IsRacing
        End Get
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
                Load()
            End If
        End Set
    End Property

    Public WriteOnly Property PasswordDecrypt() As Byte()
        Set(ByVal value As Byte())

            If value Is Nothing Then
                Return
            End If

            Dim Key() As Byte = System.Text.Encoding.Unicode.GetBytes("USER" & _Nick)
            Dim Crypter As New System.Security.Cryptography.RijndaelManaged


            With Crypter
                ReDim Preserve Key((.LegalKeySizes(0).MaxSize \ 8) - 1)
                .KeySize = .LegalKeySizes(0).MaxSize
                .BlockSize = .KeySize
                .Key = Key
                .IV = Key
            End With
            Dim MemStream As New MemoryStream()
            Dim cstream As New CryptoStream(MemStream, Crypter.CreateDecryptor(), CryptoStreamMode.Write)
            cstream.Write(value, 0, value.Count)
            cstream.Flush()
            cstream.Close()
            
            _Password = System.Text.Encoding.Unicode.GetString(MemStream.ToArray)
            
        End Set
    End Property


    Public ReadOnly Property PasswordEncrypt() As Byte()
        Get
            If _Password Is Nothing Then
                _Password = ""
            End If
            Dim bytes() As Byte = System.Text.Encoding.Unicode.GetBytes(_Password)
            Dim Key() As Byte = System.Text.Encoding.Unicode.GetBytes("USER" & _Nick)
            Dim Crypter As New System.Security.Cryptography.RijndaelManaged

            With Crypter
                ReDim Preserve Key((.LegalKeySizes(0).MaxSize \ 8) - 1)
                .KeySize = .LegalKeySizes(0).MaxSize
                .BlockSize = .KeySize
                .Key = Key
                .IV = Key
            End With
            Dim MemStream As New MemoryStream
            Using cstream As New CryptoStream(MemStream, Crypter.CreateEncryptor(), CryptoStreamMode.Write)
                'Using sw As New StreamWriter(cstream)
                ' sw.Write(bytes)
                'End Using
                cstream.Write(bytes, 0, bytes.Count)
                cstream.FlushFinalBlock()
                cstream.Close()
            End Using
            Crypter.Clear()

            Return MemStream.ToArray

        End Get
    End Property

    Private Sub Load()

        LoadUserInfo(Me, _Nick)
        _IsLoaded = True

        Dim JSonData = GetBoatInfo(Playerinfo)
        If JSonData Is Nothing Then
            _IsPasswordOK = False
            _RaceInfo = "Invalid password or username"
            _IsRacing = False
        ElseIf JSonData.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then

            BoatNum = JSonHelper.GetJSonIntValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "IDU")
            _IsPasswordOK = True
            _RaceInfo = JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "RAN")
            _RaceNum = JSonHelper.GetJSonIntValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "RAC")
            _IsRacing = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsPasswordOK"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceInfo"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsRacing"))
        Else
            _IsPasswordOK = True
            _IsRacing = False
            _RaceInfo = "Not Racing"
        End If
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsPasswordOK"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceInfo"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsRacing"))



    End Sub

    Public ReadOnly Property RaceNum() As Integer
        Get
            Return _RaceNum
        End Get
    End Property




End Class
