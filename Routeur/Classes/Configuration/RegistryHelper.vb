Imports Microsoft.Win32

Module RegistryHelper

    Private Const BASE_REG_PATH As String = "Software\Sbs\Routeur"
    Private Const USER_SUB_KEY As String = "Users"


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
