Imports System.Net

Module WS_Wrapper

    Private _LastUser As String = ""
    Private _LastPassword As String = ""
    Private _Cookies As New CookieContainer

    Public Function GetBoatInfo(ByVal Player As clsPlayerInfo) As Dictionary(Of String, Object)

        _LastPassword = Player.Password
        _LastUser = Player.Nick
        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/boatinfo.php?forcefmt=json"
        Dim Retstring As String = RequestPage(URL)

        Return Parse(Retstring)


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
