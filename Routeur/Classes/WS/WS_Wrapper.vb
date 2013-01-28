Imports System.Net
Imports System.IO
Imports System.Web
Imports System.Threading.Tasks

Module WS_Wrapper

    Private _LastUser As String = ""
    Private _LastPassword As String = ""
    Private _Cookies As New CookieContainer

    Public Function GetBoatInfo(ByVal Player As clsPlayerInfo) As Dictionary(Of String, Object)

        If _LastPassword <> Player.Password OrElse _LastUser <> Player.Email Then

            _LastPassword = Player.Password
            _LastUser = Player.Email

            _Cookies = New CookieContainer
        End If
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/boatinfo.php"
        If Player.NumBoat <> 0 Then
            URL &= "?select_idu=" & Player.NumBoat
        End If
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Return Parse(Retstring)
        Catch wex As WebException
            'Login error
            Return Nothing
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try

        Return Nothing

    End Function

    Private Function GetEpochFromDate(p1 As Date) As Long
        Return CLng(p1.Subtract(New DateTime(1970, 1, 1)).TotalSeconds)
    End Function

    Public Function GetRaceInfo(ByVal RAN As String) As String

        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/raceinfo.php?forcefmt=json&idrace=" & RAN
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

    Public Function GetRaceList() As List(Of VLMShortRaceInfo)

        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/raceinfo/list.php"
        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            Dim List As Dictionary(Of String, Object) = Parse(Retstring)
            Dim RetList As New List(Of VLMShortRaceInfo)

            If List.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
                Dim Races As Dictionary(Of String, Object) = CType(List(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object))

                For Each RaceName As String In Races.Keys
                    Dim Race As New VLMShortRaceInfo()
                    Dim RaceDict As Dictionary(Of String, Object) = CType(Races(RaceName), Dictionary(Of String, Object))

                    JSonHelper.LoadJSonDataToObject(Race, RaceDict)
                    RetList.Add(Race)
                Next

            End If
            Return RetList
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
            If Retstring = "" Then
                Return Nothing
            End If

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
            If wex.Response Is Nothing Then
                'Connection lost
                Return Nothing
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
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

    Private Sub GetRealProfileInfo(BI As BoatInfo, IdRace As String)
        Dim RetJSon As New Dictionary(Of String, Object)
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/realinfo/profile.php?idr=" & IdRace & "&idreals=" & BI.Id
        Dim BoatList As List(Of BoatInfo) = Nothing

        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            RetJSon = JSonParser.Parse(Retstring)
            Dim Success As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
            If Success AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
                CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("profile") Then
                Dim Profile As Dictionary(Of String, Object) = CType(GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "profile"), Dictionary(Of String, Object))
                With BI
                    .Name = CStr(GetJSonObjectValue(Profile, "boatname"))
                End With


            Else
                Return
            End If
            Return
        Catch wex As WebException
            If wex.Response Is Nothing Then
                'Connection lost
                Return
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
                'Login error
                Return
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 404 Then
                'Page not found (not yet implemented in VLM, testing only)
                Return
            Else
                MessageBox.Show(wex.Response.ToString)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try
        Return
    End Sub


    Private Sub GetRealTrack(BI As BoatInfo, IdRace As String)
        Dim RetJSon As New Dictionary(Of String, Object)
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/realinfo/tracks.php?idr=" & IdRace & "&idreals=" & BI.Id & "&starttime=" & GetEpochFromDate(Now.AddHours(-12))
        Dim BoatList As List(Of BoatInfo) = Nothing

        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            RetJSon = JSonParser.Parse(Retstring)
            Dim Success As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
            If Success AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
                CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("tracks") Then
                Dim Tracks As List(Of Object) = CType(GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "tracks"), List(Of Object))
                With BI
                    Dim T As List(Of Object) = CType(Tracks(Tracks.Count - 1), Global.System.Collections.Generic.List(Of Object))
                    If T IsNot Nothing AndAlso T.Count = 3 Then
                        .CurPos = New Coords(CLng(T(2)) / 1000, CLng(T(1)) / 1000)
                    End If
                End With


            Else
                Return
            End If
            Return
        Catch wex As WebException
            If wex.Response Is Nothing Then
                'Connection lost
                Return
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
                'Login error
                Return
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 404 Then
                'Page not found (not yet implemented in VLM, testing only)
                Return
            Else
                MessageBox.Show(wex.Response.ToString)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to parse JSon Data : " & vbCrLf & Retstring)
        End Try
        Return
    End Sub


    Function GetReals(IdRace As String) As List(Of BoatInfo)
        Dim RetJSon As New Dictionary(Of String, Object)
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/raceinfo/reals.php?idr=" & IdRace
        Dim BoatList As List(Of BoatInfo) = Nothing

        Dim Retstring As String = ""
        Try
            Retstring = RequestPage(URL)
            RetJSon = JSonParser.Parse(Retstring)
            Dim Success As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
            If Success AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
                CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("reals") Then

                Dim RealList As List(Of Object) = CType(JSonHelper.GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "reals"), List(Of Object))
                For Each Real In RealList
                    Dim Id As Long = CLng(JSonHelper.GetJSonObjectValue(Real, "idreals"))
                    Dim BI As New BoatInfo
                    With BI
                        .Id = Id
                        .Drawn = False
                        .Real = True
                    End With
                    GetRealProfileInfo(BI, IdRace)
                    GetRealTrack(BI, IdRace)
                    If BoatList Is Nothing Then
                        BoatList = New List(Of BoatInfo)
                    End If
                    BoatList.Add(BI)
                Next

            Else
                Return Nothing
            End If
            Return BoatList
        Catch wex As WebException
            If wex.Response Is Nothing Then
                'Connection lost
                Return Nothing
            ElseIf CType(wex.Response, HttpWebResponse).StatusCode = 401 Then
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

    Public Function GetTrack(RaceId As Integer, BoatNum As Integer, StartEpoch As Long) As List(Of TrackPoint)
        Dim RetJSon As New Dictionary(Of String, Object)
        Dim EpochNow As Long = CLng(Now.ToUniversalTime.Subtract(New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)
        Dim QryStartEpoch As Long = StartEpoch
        Dim QryEndEpoch As Long = Math.Min(StartEpoch + 48 * 3600, EpochNow)
        Dim TrackComplete As Boolean = False
        Dim RetList As New List(Of TrackPoint)
        Dim LastEpoch As Long = StartEpoch

        'Get SmartTracks List
        Dim URL1 As String = RouteurModel.Base_Game_Url & "/ws/boatinfo/smarttracks.php?idu=" & BoatNum & "&idr=" & RaceId & "&starttime=" & QryStartEpoch & "&endtime=" & EpochNow
        Dim Retstring1 As String = ""
        Retstring1 = RequestPage(URL1)
        RetJSon = JSonParser.Parse(Retstring1)
        Dim Success1 As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
        If Success1 AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
            CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("tracks_url") Then

            'Grab current non cached track points
            If CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("tracks") Then

                Dim Track As List(Of Object) = CType(JSonHelper.GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "tracks"), List(Of Object))
                Dim NbTracks As Integer = JSonHelper.GetJSonIntValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "nb_tracks")
                If NbTracks > 0 Then
                    For Each Pos As List(Of Object) In Track

                        Dim P As New TrackPoint
                        P.Epoch = CLng(Pos(0))
                        If P.Epoch > QryStartEpoch Then
                            P.P = New Coords(CDbl(Pos(1)) / 1000, CDbl(Pos(2)) / 1000)
                            RetList.Add(P)
                            LastEpoch = P.Epoch
                        End If
                    Next
                End If
            End If

            'Grab cached track points from tracks_cacheurl
            Dim Urls = CType(JSonHelper.GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "tracks_url"), List(Of Object))

            For Each Url In Urls

                If Url IsNot Nothing Then
                    Dim retstring As String = RequestPage(RouteurModel.Base_Game_Url & "/cache/tracks/" & CStr(Url))
                    RetJSon = JSonParser.Parse(retstring)

                    Dim Success As Boolean = JSonHelper.GetJSonBoolValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "success")
                    If Success AndAlso RetJSon.ContainsKey(JSONDATA_BASE_OBJECT_NAME) AndAlso _
                        CType(RetJSon(JSONDATA_BASE_OBJECT_NAME), Dictionary(Of String, Object)).ContainsKey("tracks") Then

                        Dim Track As List(Of Object) = CType(JSonHelper.GetJSonObjectValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "tracks"), List(Of Object))
                        Dim NbTracks As Integer = JSonHelper.GetJSonIntValue(RetJSon(JSONDATA_BASE_OBJECT_NAME), "nb_tracks")
                        If NbTracks > 0 Then
                            For Each Pos As List(Of Object) In Track

                                Dim P As New TrackPoint
                                P.Epoch = CLng(Pos(0))
                                If P.Epoch > QryStartEpoch Then
                                    P.P = New Coords(CDbl(Pos(1)) / 1000, CDbl(Pos(2)) / 1000)
                                    RetList.Add(P)
                                    LastEpoch = P.Epoch
                                End If
                            Next
                        End If
                    End If
                End If
            Next

        End If
        'While Not TrackComplete
        '    Dim URL As String = RouteurModel.Base_Game_Url & "/ws/boatinfo/tracks.php?idu=" & BoatNum & "&idr=" & RaceId & "&starttime=" & QryStartEpoch & "&endtime=" & QryEndEpoch
        '    Dim Retstring As String = ""
        '    Retstring = RequestPage(URL)
        '    RetJSon = JSonParser.Parse(Retstring)


        '    If QryEndEpoch >= EpochNow Or LastEpoch < QryStartEpoch Then
        '        TrackComplete = True
        '    Else
        '        QryStartEpoch = LastEpoch + 1
        '        QryEndEpoch = Math.Min(QryStartEpoch + 48 * 3600, EpochNow)
        '    End If

        'End While
        Return RetList

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
        Dim URL As String = RouteurModel.Base_Game_Url & "/ws/playerinfo/fleet_private.php"
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
        Try
            wr = Http.GetResponse()
            Dim rs = New System.IO.StreamReader(wr.GetResponseStream)
            Dim Response As String = rs.ReadToEnd
            rs.Close()

            If Response.Contains("""success"":true") Then
                Return True
            Else

                Return False
            End If
        Catch ioex As IOException
            Using fs As New StreamWriter("IOExceptionLog.log", True)
                fs.WriteLine(Now.ToString & ";" & ioex.Message & ";" & URL)
                fs.Close()
                Return False
            End Using
        Catch ex As Exception
            Return False
        End Try

    End Function


    Private Function RequestPage(ByVal URL As String) As String

        Try
            Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(URL), HttpWebRequest)
            Http.Credentials = New NetworkCredential(_LastUser, _LastPassword)
            Dim wr As WebResponse
            'Dim Encoder As New System.Text.ASCIIEncoding
            Dim rs As System.IO.StreamReader
            Dim Response As String
            Http.UserAgent = GetRouteurUserAgent()
            Http.CookieContainer = _Cookies
            Http.Accept = "application/json"
            wr = Http.GetResponse()
            rs = New System.IO.StreamReader(wr.GetResponseStream())
            Response = rs.ReadToEnd

            Return Response
        Catch ex As IOException

            Using fs As New StreamWriter("IOExceptionLog.log", True)
                fs.WriteLine(Now.ToString & ";" & ex.Message & ";" & URL)
                fs.Close()
            End Using
        End Try

        Return ""

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

    Function EngageBoatInRace(Boat As RegistryPlayerInfo, RaceID As String) As Boolean

        Dim Request As New Dictionary(Of String, Object)
        Dim verb As String = "race_subscribe"
        Request.Add("idu", Boat.IDU)
        Request.Add("idr", RaceID)

        _LastPassword = Boat.Password
        _LastUser = Boat.Email

        Return WS_Wrapper.PostBoatSetup(Boat.IDU, verb, GetStringFromJsonObject(Request))
    End Function










End Module
