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

Imports System.ComponentModel
Imports System.Security.Cryptography
Imports System.IO
Imports System.Collections.ObjectModel

Public Class RegistryPlayerInfo
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Nick As String
    Private _idu As Integer
    Private _IsMine As Boolean
    Private _Password As String
    'Private _IsLoaded As Boolean = False
    Private _RaceInfo As String = ""
    Private _IsRacing As Boolean = False
    Private _IsPasswordOK As Boolean = False
    Private _RaceNum As String
    Private _NewStyle As Boolean = False
    Private _IDP As String
    Private _Email As String
    Private _BoatList As ObservableCollection(Of BoatInfo) = Nothing
    Private _isLoading As Boolean = False
    Private _Deleted As Boolean = False

    Public Class BoatInfo
        Public IDB As Integer
        Public BoatName As String
        Public Info As Dictionary(Of String, Object)
    End Class

    Public Sub New(ByVal Name As String)

        _Nick = Name
        Load()

    End Sub

    Public Property BoatList() As ObservableCollection(Of BoatInfo)
        Get
            If NewStyle AndAlso _BoatList Is Nothing Then
                Dim Objet As Dictionary(Of String, Object) = WS_Wrapper.GetUserFleetInfo(Playerinfo.Email, Playerinfo.Password)

            End If
            Return _BoatList
        End Get
        Set(ByVal value As ObservableCollection(Of BoatInfo))
            _BoatList = value
        End Set
    End Property

    Public Property TrackColor As Color

    Public Property Deleted() As Boolean
        Get
            Return _Deleted
        End Get
        Set(ByVal value As Boolean)
            _Deleted = value
        End Set
    End Property

    Public Property Email() As String
        Get
            Return _Email
        End Get
        Set(ByVal value As String)
            _Email = value
        End Set
    End Property

    Public Property IDP() As String
        Get
            Return _IDP
        End Get
        Set(ByVal value As String)
            _IDP = value
        End Set
    End Property

    Public Property IDU() As Integer
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
            Return _idu
        End Get
        Set(ByVal value As Integer)

            If _idu <> value Then
                _idu = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IDU"))
            End If


        End Set
    End Property

    Public Property IsLoading() As Boolean
        Get
            Return _isLoading
        End Get
        Set(ByVal value As Boolean)
            _isLoading = value
        End Set
    End Property

    Public ReadOnly Property IsPasswordOK() As Boolean
        Get
            Return _IsPasswordOK
        End Get
    End Property

    
    Public Property NewStyle() As Boolean
        Get
            Return _NewStyle
        End Get
        Set(ByVal value As Boolean)
            _NewStyle = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NewStyle"))
        End Set
    End Property

    Public ReadOnly Property RaceInfo() As String
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
            Return _RaceInfo
        End Get

    End Property

    Public ReadOnly Property IsRacing() As Boolean
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
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
                .Email = email
                .NumBoat = IDU
                .Nick = Nick
                .Password = Password
                .TrackColor = TrackColor
            End With
            Return Ret
        End Get
    End Property

    Public Property IsMine() As Boolean
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
            Return _IsMine
        End Get
        Set(ByVal value As Boolean)
            If _IsMine <> value Then
                _IsMine = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsMine"))
            End If
        End Set
    End Property

    Public Property Nick() As String
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
            Return _Nick
        End Get
        Set(ByVal value As String)

            If _Nick <> value Then
                _Nick = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Nick"))
            End If
        End Set
    End Property

    Public Property Password() As String
        Get
            'If Not _IsLoaded Then
            'Load()
            'End If
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

        'If _IsLoaded OrElse IsLoading Then
        If IsLoading Then
            Return
        End If

        LoadUserInfo(Me, _Nick)
        '_IsLoaded = True

        Dim JSonData = GetBoatInfo(Playerinfo)
        If JSonData Is Nothing Then
            _IsPasswordOK = False
            _RaceInfo = "Invalid password or username"
            _IsRacing = False
            _NewStyle = False
        ElseIf JSonData.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then

            _IDP = JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "IDP")
            _idu = JSonHelper.GetJSonIntValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "IDU")
            _IsPasswordOK = True
            _RaceInfo = JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "RAN")
            _RaceNum = JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "RAC")
            _IsRacing = _RaceNum <> "0"
            Dim lcolor As Long = 0
            Long.TryParse(JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "COL"), System.Globalization.NumberStyles.HexNumber, Nothing, lcolor)
            TrackColor = Color.FromRgb(CByte(lcolor And &HFF), CByte((lcolor And &HFF00) >> 8), CByte((lcolor And &HFF0000) >> 16))
            'If Not NewStyle Then
            '    _Email = JSonHelper.GetJSonStringValue(JSonData(JSONDATA_BASE_OBJECT_NAME), "EML")
            'End If
            '_NewStyle = _IDP.Contains("@"c)
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
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IDU"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IDP"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NewStyle"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Email"))

        If NewStyle Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatList"))
        End If


    End Sub

    Public ReadOnly Property RaceNum() As String
        Get
            Return _RaceNum
        End Get
    End Property


    Protected Overrides Sub Finalize()
        If Not Deleted Then
            SaveUserInfo(Me)
        End If
        MyBase.Finalize()
    End Sub
End Class
