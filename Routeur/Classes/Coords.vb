Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml
Imports System.Math
Imports System.Xml.Serialization



Public Class Coords
    Implements ICoords
    'Implements INotifyPropertyChanged


    'Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Enum NORTH_SOUTH As Integer
        N = 1
        S = -1
    End Enum

    Public Enum EAST_WEST As Integer
        E = 1
        W = -1
    End Enum

    Private _Lat As Double
    Private _n_lat As Double
    Private _Lon As Double
    'Private _Name As String
    Private _HashCode As Long = 0


    Public Property Lat() As Double Implements ICoords.Lat
        Get
            Return _Lat
        End Get
        Set(ByVal value As Double)
            _Lat = value
            _n_lat = _Lat Mod (PI / 2)
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat"))
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat_Deg"))
        End Set
    End Property

    <XmlIgnore()> _
    Public Property Lat_Deg() As Double
        Get
            Return _Lat * 180 / Math.PI
        End Get
        Set(ByVal value As Double)
            _Lat = value / 180 * Math.PI
            _n_lat = _Lat Mod (PI / 2)
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat"))
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat_Deg"))
        End Set
    End Property

    Public Property Lon() As Double Implements ICoords.Lon
        Get
            Return _Lon
        End Get
        Set(ByVal value As Double)

            'While value > Math.PI
            '    value -= 2 * Math.PI
            'End While

            'While value < -Math.PI
            '    value += 2 * Math.PI

            'End While
            If Double.IsNaN(value) Then
                Dim bp As Integer = 0
            End If
            _Lon = value
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon"))
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon_Deg"))
        End Set
    End Property

    <XmlIgnore()> _
    Public Property Lon_Deg() As Double
        Get
            Return _Lon * 180 / Math.PI
        End Get
        Set(ByVal value As Double)


            Lon = value / 180 * Math.PI
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon"))
            'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon_Deg"))
        End Set
    End Property

    'Public Property Name() As String
    '    Get
    '        Return _Name
    '    End Get
    '    Set(ByVal value As String)
    '        _Name = value
    '        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
    '    End Set
    'End Property

    Public Shared Operator =(ByVal v1 As Coords, ByVal v2 As Coords) As Boolean

        If v1 Is Nothing And v2 Is Nothing Then
            Return True
        ElseIf v1 Is Nothing Or v2 Is Nothing Then
            Return False
        Else
            Return v1.Lat = v2.Lat AndAlso v1.Lon = v2.Lon
        End If

    End Operator

    Public Shared Operator <>(ByVal v1 As Coords, ByVal v2 As Coords) As Boolean
        Return Not v1 = v2
    End Operator

    <XmlIgnore()> _
    Public ReadOnly Property N_Lon_Deg As Double
        Get
            Return N_Lon / PI * 180
        End Get
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property N_Lon As Double Implements ICoords.N_Lon
        Get
            If Lon < -PI Or Lon > PI Then
                Return ((Lon + 3 * PI) Mod (2 * PI)) - PI
            Else
                Return Lon
            End If

        End Get
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property N_Lat As Double Implements ICoords.N_Lat
        Get
            Return _n_lat
        End Get
    End Property


    Public Sub RoundTo(ByVal RoundingGrain As Double)

        If RoundingGrain = 0 Then
            Return
        End If

        Lat_Deg = CInt(Lat_Deg / RoundingGrain) * RoundingGrain
        Lon_Deg = CInt(Lon_Deg / RoundingGrain) * RoundingGrain

        Return

    End Sub


    Public ReadOnly Property Mercator_X_Deg() As Double
        Get
            Return Lon_Deg
        End Get
    End Property

    Public Shared ReadOnly Property Mercator_Y_Deg(Lat_Deg As Double) As Double
        Get
            Dim ret As Double = Log(Tan(Lat_Deg) + 1 / Cos(Lat_Deg)) / PI * 180
            'Debug.Assert(Not Double.IsNaN(ret))
            If ret < -90 Then
                ret = -90
            ElseIf ret > 90 Then
                ret = 90
            End If
            Return ret

        End Get
    End Property

    Public ReadOnly Property Mercator_Y_Deg() As Double
        Get
            Dim ret As Double = Log(Tan(N_Lat) + 1 / Cos(N_Lat)) / PI * 180
            'Debug.Assert(Not Double.IsNaN(ret))
            If ret < -90 Then
                ret = -90
            ElseIf ret > 90 Then
                ret = 90
            End If
            Return ret
        End Get
    End Property


    Public Function [Module]() As Double

        Return Sqrt(X * X + Y * Y + Z * Z)

    End Function

    Public Sub New()

    End Sub

    Public Sub New(ByVal C As ICoords)

        If C Is Nothing Then
            Return
        End If
        Lat = C.Lat
        Lon = C.Lon

    End Sub

    Public Sub New(ByVal C As Coords)
        If C Is Nothing Then
            Return
        End If
        Lat = C.Lat
        Lon = C.Lon

    End Sub

    Public Sub New(ByVal Lat_Deg As Double, ByVal LonDeg As Double)
        Me.New(CType(Lat_Deg, Decimal), CType(LonDeg, Decimal))
    End Sub

    Public Sub New(ByVal Lat_Deg As Decimal, ByVal LonDeg As Decimal)

        'If Lat_Deg > 90 Then
        '    Me.Lat_Deg = Lat_Deg - 180
        'ElseIf Lat_Deg < -90 Then
        '    Me.Lat_Deg = Lat_Deg + 180
        'Else
        Me.Lat_Deg = Lat_Deg
        'End If

        'If LonDeg > 180 Then
        '    Me.Lon_Deg = LonDeg - 360
        'ElseIf LonDeg < -180 Then
        '    Me.Lon_Deg = LonDeg + 360
        'Else
        Me.Lon_Deg = LonDeg
        'End If


    End Sub

    Public Sub New(ByVal Lat_Deg As Integer, ByVal Lat_Min As Integer, ByVal Lat_Sec As Integer, ByVal NS As NORTH_SOUTH, ByVal Lon_Deg As Integer, ByVal Lon_Min As Integer, ByVal Lon_Sec As Integer, ByVal EW As EAST_WEST)

        Me.Lat_Deg = (CInt(NS) * (Lat_Deg + Lat_Min / 60 + Lat_Sec / 3600)) ' Mod 180
        Me.Lon_Deg = (CInt(EW) * (Lon_Deg + Lon_Min / 60 + Lon_Sec / 3600)) ' Mod 180

    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)

        Lat = Atan2(z, Sqrt(x ^ 2 + y ^ 2))
        _Lon = -Atan2(-y, x)

    End Sub

    'Public Sub New(ByVal P As XmlElement)

    '    For Each A As XmlAttribute In P.Attributes

    '        Select Case A.Name
    '            Case "Lat"
    '                Lat = Val(A.Value)
    '            Case "Lon"
    '                Lon = Val(A.Value)

    '            Case "Name"
    '                Name = CStr(A.Value)
    '        End Select
    '    Next
    'End Sub

    Public Sub Normalize()

        Dim M As Double = [Module]()
        Dim _x As Double = X / M
        Dim _y As Double = Y / M
        Dim _z As Double = Z / M

        Lat = Atan2(Z, Sqrt(_x ^ 2 + _y ^ 2))
        _Lon = -Atan2(-_y, _x)

    End Sub

    Public Overrides Function ToString() As String

        Static Cv As New CoordsConverter

        Return CStr(Cv.Convert(Me, GetType(String), 1, System.Threading.Thread.CurrentThread.CurrentUICulture))

    End Function

    Public ReadOnly Property X() As Double
        Get
            Return Cos(_Lat) * Cos(-_Lon)
        End Get
    End Property

    Public ReadOnly Property Y() As Double
        Get
            Return -Cos(Lat) * Sin(-Lon)
        End Get
    End Property

    Public ReadOnly Property Z() As Double
        Get
            Return Sin(Lat)
        End Get
    End Property

    Public Function Equals1(ByVal C As ICoords) As Boolean Implements ICoords.Equals
        Return CoordsComparer.Equals1(Me, C)
    End Function

    Public Function GetHashCode1() As Long Implements ICoords.GetHashCode
        If _HashCode = 0 Then
            _HashCode = CoordsComparer.GetHashCode1(Me)
        End If
        Return _HashCode
    End Function
End Class

'Public Class WayPoints

'    Public Points As New ObservableCollection(Of Coords)

'    Private Sub LoadPointList(ByVal Points As XmlElement)

'        For Each P In Points.ChildNodes

'            If TypeOf P Is XmlElement AndAlso CType(P, XmlElement).Name = "Point" Then
'                Dim NewP As New Coords(CType(P, XmlElement))
'                Me.Points.Add(NewP)
'            End If

'        Next

'    End Sub


'    Public Sub New(ByVal Doc As XmlDataProvider)

'        Doc.Refresh()
'        Points.Clear()

'        For Each node In Doc.Document.ChildNodes


'            If TypeOf node Is XmlElement Then
'                If CType(node, XmlElement).Name = "Points" Then
'                    LoadPointList(CType(node, XmlElement))
'                End If
'            End If


'        Next

'    End Sub

'End Class


