Imports System.Net

Module WS_Wrapper

    Private _LastUser As String = ""
    Private _LastPassword As String = ""
    Private _Cookies As New CookieContainer

    Public Function GetBoatInfo(ByVal Player As clsPlayerInfo) As Dictionary(Of String, Object)

        If _LastPassword <> Player.Password OrElse _LastUser <> Player.Nick Then
            _LastPassword = Player.Password
            _LastUser = Player.Nick
            _Cookies = New CookieContainer
        End If
        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/boatinfo.php?forcefmt=json"
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Return Parse(Retstring)
        Catch wex As WebException
            If CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
                'Login error
                Return Nothing
            Else
                MessageBox.Show(wex.Response.ToString)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try

        Return Nothing

    End Function

    Public Function GetRaceInfo(ByVal RAN As Integer) As Dictionary(Of String, Object)

        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/raceinfo.php?forcefmt=json&idrace=" & RAN.ToString
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Return Parse(Retstring)
        Catch wex As WebException
            If CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
                'Login error
                Return Nothing
            Else
                MessageBox.Show(wex.Response.ToString)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try

        Return Nothing
    End Function

    Private Function RequestPage(ByVal URL As String) As String

        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(URL), HttpWebRequest)
        Http.Credentials = New NetworkCredential(_LastUser, _LastPassword)
        Dim wr As WebResponse
        'Dim Encoder As New System.Text.ASCIIEncoding
        Dim rs As System.IO.StreamReader
        Dim Response As String
        Http.CookieContainer = _Cookies
        wr = Http.GetResponse()
        rs = New System.IO.StreamReader(wr.GetResponseStream())
        Response = rs.ReadToEnd

        Return Response
        
    End Function



End Module
