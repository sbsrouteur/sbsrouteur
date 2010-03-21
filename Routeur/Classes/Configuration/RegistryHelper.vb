Imports Microsoft.Win32

Module RegistryHelper

    Private Const BASE_REG_PATH As String = "Software\Sbs\Routeur"
    Private Const USER_SUB_KEY As String = "Users"
    Private Const USER_NICK As String = "Nick"
    Private Const USER_NUM As String = "NumBoat"
    Private Const USER_PASSWORD As String = "Password"
    Private Const USER_MINE As String = "ISMINE"


    Public Sub DeleteUser(ByVal P As RegistryPlayerInfo)

        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH & "\" & USER_SUB_KEY, True)

        If Not Reg Is Nothing Then
            Reg.DeleteSubKey(P.Nick)
        End If

        Return

    End Sub


    Public Function GetUserPassword(ByVal UserName As String) As String

        Dim PasswordKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(PasswordKey, False)
        Dim RetString As String = ""

        If Reg Is Nothing Then
            Return ""
        End If

        Dim Value As Object = Reg.GetValue(UserName)

        If Value Is Nothing Then
            Return ""
        Else
            Return CStr(Value)
        End If

        Return RetString

    End Function

    Public Sub LoadPlayers(ByVal List As IList(Of RegistryPlayerInfo))

        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH & "\" & USER_SUB_KEY, False)

        If Reg Is Nothing Then
            Return
        End If
        Dim names() As String = Reg.GetSubKeyNames()

        For Each Name In names
            Dim P As RegistryPlayerInfo = New RegistryPlayerInfo(Name)
            List.Add(P)
        Next

        Return


    End Sub

    Public Sub LoadUserInfo(ByVal User As RegistryPlayerInfo, ByVal BoatNick As String)
        If BoatNick Is Nothing Then
            Return
        End If
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & BoatNick
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, False)

        If Reg Is Nothing Then
            Return
        End If

        With User
            .Nick = BoatNick
            .BoatNum = CInt(Reg.GetValue(USER_NUM, 0))
            .IsMine = CInt(Reg.GetValue(USER_MINE, 0)) = 1
            .PasswordDecrypt = CType(Reg.GetValue(USER_PASSWORD, Nothing), Byte())
        End With

        Return
    End Sub

    Public Sub SaveUserInfo(ByVal User As RegistryPlayerInfo)

        If User.Nick Is Nothing Then
            Return
        End If
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & User.Nick
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, True)

        If Reg Is Nothing Then
            Reg = Registry.CurrentUser.CreateSubKey(UserKey)
        End If

        With User

            Reg.SetValue(USER_NUM, .BoatNum, RegistryValueKind.DWord)
            Reg.SetValue(USER_MINE, If(.IsMine, 1, 0), RegistryValueKind.DWord)
            Reg.SetValue(USER_PASSWORD, .PasswordEncrypt, RegistryValueKind.Binary)
        End With

        Return
    End Sub


    Public Function SetUserPassword(ByVal UserName As String, ByVal Password As String) As Boolean

        Dim PasswordKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(PasswordKey, True)
        Dim RetString As String = ""

        If Reg Is Nothing Then
            Reg = Registry.CurrentUser.CreateSubKey(PasswordKey, RegistryKeyPermissionCheck.ReadWriteSubTree)
            If Reg Is Nothing Then
                Return False
            End If
        End If

        Try
            Reg.SetValue(UserName, Password, RegistryValueKind.String)
            Return True
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbCrLf & ex.StackTrace, "Error setting password")
        End Try
        Return False


    End Function


End Module
