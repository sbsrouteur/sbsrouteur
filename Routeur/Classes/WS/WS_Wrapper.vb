Imports System.Net
Imports System.IO
Imports System.Web

Module WS_Wrapper

    Private _LastUser As String = ""
    Private _LastPassword As String = ""
    Private _Cookies As New CookieContainer

    Public Function GetBoatInfo(ByVal Player As clsPlayerInfo) As Dictionary(Of String, Object)

        If _LastPassword <> Player.Password OrElse _LastUser <> Player.Nick Then
            If Not Player.NewStyle Then
                'old style login
                _LastPassword = Player.Password
                _LastUser = Player.Nick
            Else
                _LastPassword = Player.Password
                _LastUser = Player.Email
            End If
            _Cookies = New CookieContainer
        End If
        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/boatinfo.php?forcefmt=json"
        If Player.NewStyle Then
            URL &= "&select_idu=" & Player.NumBoat
        End If
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Return Parse(Retstring)
        Catch wex As WebException
            If wex.Response Is Nothing OrElse CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
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

    Public Function GetRaceInfo(ByVal RAN As String) As String

        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/raceinfo.php?forcefmt=json&idrace=" & RAN
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Return Retstring
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

    Public Function GetRankings(ByVal IdRace As String, ByRef ArrivedCount As Integer) As Dictionary(Of String, Object)

        Dim RetJSon As New Dictionary(Of String, Object)
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/raceinfo/ranking.php?idr=" & IdRace

        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            RetJSon = JSonParser.Parse(Retstring)
            Dim Success As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
            If Success AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
                CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("ranking") Then

                If CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("nb_arrived") Then
                    ArrivedCount = JSonHelper.GetJSonIntValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "nb_arrived")
                End If

                Return CType(JSonHelper.GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "ranking"), Dictionary(Of String, Object))
            Else
                Return Nothing
            End If
            Return RetJSon
        Catch wex As WebException
            If CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
                'Login error
                Return Nothing
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 404 Then
                'Page not found (not yet implemented in VLM, testing only)
                Return Nothing
            Else
                MessageBox.Show(wex.Response.ToString)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try
        Return Nothing
    End Function

    Public Function GetRouteurUserAgent() As String
        If My.Application IsNot Nothing Then
            Return "SbsRouteur/" & My.Application.Info.Version.ToString
        Else
            Return "SbsRouteur/??"
        End If
    End Function

    Public Function GetUserFleetInfo(ByVal UserName As String, ByVal Password As String) As Dictionary(Of String, Object)

        If _LastPassword <> Password OrElse _LastUser <> UserName Then
            If Not UserName.Contains("@"c) Then
                'old style login not supported for fleet
                Return Nothing
            Else
                _LastPassword = Password
                _LastUser = UserName
            End If
            _Cookies = New CookieContainer
        End If
        Dim URL As String = RouteurModel.BASE_GAME_URL & "/ws/playerinfo/fleet_private.php"
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Dim RetObject As Dictionary(Of String, Object) = Parse(Retstring)
            If RetObject IsNot Nothing AndAlso RetObject.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
                Return CType(RetObject(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object))
            Else
                Return RetObject
            End If
        Catch wex As WebException
            If wex.Response Is Nothing OrElse CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
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

    Public Function PostBoatSetup(ByVal idu As Integer, ByVal Verb As String, ByVal Data As String) As Boolean

        Dim RetVal As Boolean = False
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/boatsetup/" & Verb & ".php?select_idu=" & idu '?parms=" & Data

        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(URL), HttpWebRequest)

        Http.Credentials = New NetworkCredential(_LastUser, _LastPassword)
        Http.UserAgent = GetRouteurUserAgent()
        Http.CookieContainer = _Cookies
        Http.Method = "POST"
        Http.ContentType = "application/x-www-form-urlencoded"
        Dim BufStr As String = "parms=" & HttpUtility.UrlEncode(Data)
        'Dim bufstr = Data
        'Dim enc As New System.Text.ASCIIEncoding
        Dim dataBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(BufStr)
        'Dim dataBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(BufStr)
        'Http.ContentLength = dataBytes.Count
        Dim Sr As Stream = Http.GetRequestStream
        Sr.Write(dataBytes, 0, dataBytes.Count)
        Sr.Flush()
        Sr.Close()


        Dim wr As WebResponse
        wr = Http.GetResponse()
        Dim rs = New System.IO.StreamReader(wr.GetResponseStream)
        Dim Response As String = rs.ReadToEnd
        rs.Close()

        If Response.Contains("""success"":true") Then
            Return True
        Else

            Return False
        End If
        

    End Function


    Private Function RequestPage(ByVal URL As String) As String

        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(URL), HttpWebRequest)
        Http.Credentials = New NetworkCredential(_LastUser, _LastPassword)
        Dim wr As WebResponse
        'Dim Encoder As New System.Text.ASCIIEncoding
        Dim rs As System.IO.StreamReader
        Dim Response As String
        Http.UserAgent = GetRouteurUserAgent()
        Http.CookieContainer = _Cookies
        wr = Http.GetResponse()
        rs = New System.IO.StreamReader(wr.GetResponseStream())
        Response = rs.ReadToEnd

        Return Response

    End Function

    Public Function SetBoatHeading(ByVal idu As Integer, ByVal Heading As Double) As Boolean

        Dim RetValue As Boolean = False
        Dim Request As New Dictionary(Of String, Object)
        Dim verb As String = "pilot_set"
        Request.Add("idu", idu)
        Request.Add("pim", 1)
        Request.Add("pip", Heading)

        Return WS_Wrapper.PostBoatSetup(idu, verb, GetStringFromJsonObject(Request))


    End Function

    Public Sub SetCredential(ByVal User As String, ByVal Password As String)
        _LastPassword = Password
        _LastUser = User

    End Sub
    Public Function SetPIM(ByVal idu As Integer, ByVal PIM As Integer) As Boolean

        Dim RetValue As Boolean = False
        Dim Request As New Dictionary(Of String, Object)
        Dim verb As String = "pilot_set"
        Request.Add("idu", idu)
        Request.Add("pim", PIM)

        Return WS_Wrapper.PostBoatSetup(idu, verb, GetStringFromJsonObject(Request))
    End Function


    Public Function SetWindAngle(ByVal idu As Integer, ByVal Angle As Double) As Boolean

        Dim RetValue As Boolean = False
        Dim Request As New Dictionary(Of String, Object)
        Dim verb As String = "pilot_set"
        Request.Add("idu", idu)
        Request.Add("pim", 2)
        Request.Add("pip", Angle)

        Return WS_Wrapper.PostBoatSetup(idu, verb, GetStringFromJsonObject(Request))
    End Function

    Public Function SetWP(ByVal Idu As Integer, ByVal WP As Coords) As Boolean

        If WP Is Nothing Then
            WP = New Coords
        End If
        '"pip":{"targetlat":42.8,"targetlong":-7,"targetandhdg":180}
        Dim pip As New Dictionary(Of String, Object)
        pip.Add("targetlat", WP.Lat_Deg)
        pip.Add("targetlong", WP.Lon_Deg)
        pip.Add("targetandhdg", -1)

        Dim Request As New Dictionary(Of String, Object)
        Dim verb As String = "target_set"
        Request.Add("idu", Idu)
        Request.Add("pip", pip)
        Request.Add("debug", True)

        Return WS_Wrapper.PostBoatSetup(Idu, verb, GetStringFromJsonObject(Request))

    End Function




End Module
