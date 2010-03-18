Imports Microsoft.Win32

Module RegistryHelper

    Private Const BASE_REG_PATH As String = "Software\Sbs\Routeur"
    Private Const USER_SUB_KEY As String = "Users"
    Private Const USER_NICK As String = "Nick"
    Private Const USER_NUM As String = "NumBoat"
    Private Const USER_PASSWORD As String = "Password"
    Private Const USER_MINE As String = "ISMINE"




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

    Public Sub LoadUserInfo(ByVal User As RegistryPlayerInfo, ByVal BoatNum As Integer)
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & BoatNum.ToString
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, False)

        If Reg Is Nothing Then
            Return
        End If

        With User
            .Nick = CStr(Reg.GetValue(USER_NICK, "???"))
            .IsMine = CInt(Reg.GetValue(USER_MINE, 0)) = 1
            .Password = CStr(Reg.GetValue(USER_PASSWORD, "???"))
        End With

        Return
    End Sub

    Public Sub SaveUserInfo(ByVal User As RegistryPlayerInfo)
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & User.BoatNum.ToString
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, True)

        If Reg Is Nothing Then
            Reg = Registry.CurrentUser.CreateSubKey(UserKey)
        End If

        With User

            Reg.SetValue(USER_NICK, .Nick, RegistryValueKind.String)
            Reg.SetValue(USER_MINE, If(.IsMine, 1, 0), RegistryValueKind.DWord)
            Reg.SetValue(USER_PASSWORD, .Password, RegistryValueKind.String)
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
