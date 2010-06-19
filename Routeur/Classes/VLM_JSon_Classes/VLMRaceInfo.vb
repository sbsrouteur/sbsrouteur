Public Class VLMRaceInfo

    Private _idraces As Integer
    Private _RaceName As String
    Private _StartLon As Integer
    Private _StartLat As Integer
    Private _BoatType As String
    Private _StartDate As DateTime
    Private _race_waypoints As New List(Of VLM_RaceWaypoint)

    Public Property boattype() As String
        Get
            Return _BoatType
        End Get
        Set(ByVal value As String)
            _BoatType = value
        End Set
    End Property

    Public Property idraces() As Integer
        Get
            Return _idraces
        End Get
        Set(ByVal value As Integer)
            _idraces = value
        End Set
    End Property

    Public ReadOnly Property races_waypoints(ByVal index As Integer) As VLM_RaceWaypoint
        Get

            If index <= 0 Then
                Dim DestIndex As Integer = RouteurModel.CurWP - 1
                If DestIndex < 0 Then
                    DestIndex = 0
                ElseIf DestIndex >= _race_waypoints.Count Then
                    DestIndex = _race_waypoints.Count - 1
                End If
                Return _race_waypoints(DestIndex)
            ElseIf index <= _race_waypoints.Count Then
                Return _race_waypoints(index - 1)
            Else
                Return _race_waypoints(_race_waypoints.Count - 1)
            End If

        End Get
        
    End Property

    Public Property races_waypoints() As List(Of VLM_RaceWaypoint)
        Get
            Return _race_waypoints
        End Get
        Set(ByVal value As List(Of VLM_RaceWaypoint))
            _race_waypoints = value
        End Set
    End Property

    Public ReadOnly Property Route() As Coords()
        Get
            Dim R(_race_waypoints.Count - 1) As Coords
            Dim i As Integer
            For i = 0 To _race_waypoints.Count - 1
                R(i) = _race_waypoints(i).WPs(0)(0)
            Next

            Return R

        End Get
    End Property

    Public Property racename() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
        End Set
    End Property

    Public Property deptime() As DateTime
        Get
            Return _StartDate
        End Get
        Set(ByVal value As DateTime)
            _StartDate = value
        End Set
    End Property

    Public ReadOnly Property Start() As Coords
        Get
            Return New Coords(startlat / 1000, startlong / 1000)
        End Get
    End Property

    Public Property startlat() As Integer
        Get
            Return _StartLat
        End Get
        Set(ByVal value As Integer)
            _StartLat = value
        End Set
    End Property

    Public Property startlong() As Integer
        Get
            Return _StartLon
        End Get
        Set(ByVal value As Integer)
            _StartLon = value
        End Set
    End Property

End Class
