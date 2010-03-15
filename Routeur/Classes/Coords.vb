Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml
Imports System.Math


Public Class CoordsComparer
    Implements IComparer(Of Coords)
    Implements IEqualityComparer(Of Coords)
    Private COORD_COMPARE_GRAIN As Double = 1 / RouteurModel.GridGrain * 16

    

    Public Function Compare(ByVal x As Coords, ByVal y As Coords) As Integer Implements System.Collections.Generic.IComparer(Of Coords).Compare
        If x.Lat > y.Lat Then
            Return 1
        ElseIf x.Lat < y.Lat Then
            Return -1
        Else
            If x.Lon > y.Lon Then
                Return 1
            ElseIf x.Lon < y.Lon Then
                Return -1
            Else
                Return 0

            End If
        End If
    End Function

    Public Function Equals1(ByVal x As Coords, ByVal y As Coords) As Boolean Implements System.Collections.Generic.IEqualityComparer(Of Coords).Equals
        If Double.IsNaN(x.Lat) Or Double.IsNaN(x.Lon) Or Double.IsNaN(y.Lat) Or Double.IsNaN(y.Lon) Then
            Return False
        End If
        Return GetHashCode1(x) = GetHashCode1(y)
        'Return (CInt(x.Lat_DegCOORD_COMPARE_GRAIN * ) = CInt(COORD_COMPARE_GRAIN * y.Lat_Deg) AndAlso CInt(COORD_COMPARE_GRAIN * x.Lon_Deg) = CInt(COORD_COMPARE_GRAIN * y.Lon_Deg))
    End Function

    Public Function GetHashCode1(ByVal obj As Coords) As Integer Implements System.Collections.Generic.IEqualityComparer(Of Coords).GetHashCode

        Return CInt(CInt((obj.Lat_Deg + 90) * COORD_COMPARE_GRAIN) + (CInt(COORD_COMPARE_GRAIN * (obj.Lon_Deg + 180) * 360)))

    End Function

End Class

Public Class Coords

    Implements INotifyPropertyChanged


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Enum NORTH_SOUTH As Integer
        N = 1
        S = -1
    End Enum

    Public Enum EAST_WEST As Integer
        E = -1
        W = 1
    End Enum

    Private _Lat As Double
    Private _Lon As Double
    Private _Name As String

    Public Property Lat() As Double
        Get
            Return _Lat
        End Get
        Set(ByVal value As Double)
            _Lat = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat_Deg"))
        End Set
    End Property

    Public Property Lat_Deg() As Double
        Get
            Return _Lat * 180 / Math.PI
        End Get
        Set(ByVal value As Double)
            _Lat = value / 180 * Math.PI
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lat_Deg"))
        End Set
    End Property

    Public Property Lon() As Double
        Get
            Return _Lon
        End Get
        Set(ByVal value As Double)

            While value > Math.PI
                value -= Math.PI * Math.Floor(value / Math.PI)
            End While

            While value < -Math.PI
                value += Math.PI

            End While
            _Lon = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon_Deg"))
        End Set
    End Property

    Public Property Lon_Deg() As Double
        Get
            Return _Lon * 180 / Math.PI
        End Get
        Set(ByVal value As Double)

            
            Lon = value / 180 * Math.PI
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Lon_Deg"))
        End Set
    End Property

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property

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

    Public Sub RoundTo(ByVal RoundingGrain As Double)

        If RoundingGrain = 0 Then
            Return
        End If

        Lat_Deg = CInt(Lat_Deg / RoundingGrain) * RoundingGrain
        Lon_Deg = CInt(Lon_Deg / RoundingGrain) * RoundingGrain

        Return

    End Sub

    Public ReadOnly Property Mercator_X() As Double
        Get
            Return Lon
        End Get
    End Property

    Public ReadOnly Property Mercator_Y() As Double
        Get
            Return Log(Tan(Lat) + 1 / Cos(Lat))
        End Get
    End Property


    Public Function [Module]() As Double

        Return Sqrt(X * X + Y * Y + Z * Z)

    End Function

    Public Sub New()

    End Sub

    Public Sub New(ByVal C As Coords)

        Lat = C.Lat
        Lon = C.Lon

    End Sub

    Public Sub New(ByVal Lat_Deg As Double, ByVal LonDeg As Double)
        Me.New(CType(Lat_Deg, Decimal), CType(LonDeg, Decimal))
    End Sub

    Public Sub New(ByVal Lat_Deg As Decimal, ByVal LonDeg As Decimal)

        If Lat_Deg > 90 Then
            Me.Lat_Deg = Lat_Deg - 180
        ElseIf Lat_Deg < -90 Then
            Me.Lat_Deg = Lat_Deg + 180
        Else
            Me.Lat_Deg = Lat_Deg
        End If

        If LonDeg > 180 Then
            Me.Lon_Deg = LonDeg - 360
        ElseIf LonDeg < -180 Then
            Me.Lon_Deg = LonDeg + 360
        Else
            Me.Lon_Deg = LonDeg
        End If


    End Sub

    Public Sub New(ByVal Lat_Deg As Integer, ByVal Lat_Min As Integer, ByVal Lat_Sec As Integer, ByVal NS As North_south, ByVal Lon_Deg As Integer, ByVal Lon_Min As Integer, ByVal Lon_Sec As Integer, ByVal EW As east_west)

        Me.Lat_Deg = (CInt(NS) * (Lat_Deg + Lat_Min / 60 + Lat_Sec / 3600)) Mod 180
        Me.Lon_Deg = (CInt(EW) * (Lon_Deg + Lon_Min / 60 + Lon_Sec / 3600)) Mod 180

    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)

        _Lat = Atan2(z, Sqrt(x ^ 2 + y ^ 2))
        _Lon = -Atan2(-y, x)

    End Sub

    Public Sub New(ByVal P As XmlElement)

        For Each A As XmlAttribute In P.Attributes

            Select Case A.Name
                Case "Lat"
                    Lat = Val(A.Value)
                Case "Lon"
                    Lon = Val(A.Value)

                Case "Name"
                    Name = CStr(A.Value)
            End Select
        Next
    End Sub

    Public Sub Normalize()

        Dim M As Double = [Module]()
        Dim _x As Double = X / M
        Dim _y As Double = Y / M
        Dim _z As Double = Z / M

        _Lat = Atan2(Z, Sqrt(_x ^ 2 + _y ^ 2))
        _Lon = -Atan2(-_y, _x)

    End Sub

    Public Overrides Function ToString() As String

        Static Cv As New CoordsConverter

        Return CStr(Cv.Convert(Me, GetType(String), 1, System.Threading.Thread.CurrentThread.CurrentUICulture))

    End Function

    Public ReadOnly Property X() As Double
        Get
            Return cos(_Lat) * cos(-_Lon)
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


End Class

Public Class WayPoints

    Public Points As New ObservableCollection(Of Coords)

    Private Sub LoadPointList(ByVal Points As XmlElement)

        For Each P In Points.ChildNodes

            If TypeOf P Is XmlElement AndAlso CType(P, XmlElement).Name = "Point" Then
                Dim NewP As New Coords(CType(P, XmlElement))
                Me.Points.Add(NewP)
            End If

        Next

    End Sub

    
    Public Sub New(ByVal Doc As XmlDataProvider)

        Doc.Refresh()
        Points.Clear()

        For Each node In Doc.Document.ChildNodes


            If TypeOf node Is XmlElement Then
                If CType(node, XmlElement).Name = "Points" Then
                    LoadPointList(CType(node, XmlElement))
                End If
            End If


        Next

    End Sub

End Class


