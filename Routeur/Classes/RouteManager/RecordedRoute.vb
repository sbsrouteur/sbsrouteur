Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Xml.Serialization
Imports System.Text

Public Class RecordedRoute

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Key As Guid
    Private _RaceID As String
    Private _RaceName As String
    Private _RouteName As String
    Private _Route As ObservableCollection(Of RoutePointView)
    Private _Visible As Boolean = True
    Private _Color As Color = Color.FromRgb(CByte(Rnd() * 128 + 100), CByte(Rnd() * 128 + 100), CByte(Rnd() * 128 + 100))
    Private Shared _M As RouteurModel



    Public Sub New()
        _Key = New Guid
    End Sub

    Public Property Color() As Color
        Get
            Return _Color
        End Get
        Set(ByVal value As Color)
            _Color = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            OnShapeChange()
        End Set
    End Property


    Public ReadOnly Property ColorBrush() As SolidColorBrush
        Get
            _Color.A = 255
            Return New SolidColorBrush(Color)
        End Get
    End Property

    <XmlIgnore()> _
    Public Property ColorR() As Byte
        Get
            Return _Color.R
        End Get
        Set(ByVal value As Byte)
            _Color.R = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorR"))
            OnShapeChange()
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorG() As Byte
        Get
            Return _Color.G
        End Get
        Set(ByVal value As Byte)
            _Color.G = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorG"))
            OnShapeChange()
        End Set
    End Property

    <XmlIgnore()> _
    Public Property ColorB() As Byte
        Get
            Return _Color.B
        End Get
        Set(ByVal value As Byte)
            _Color.B = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Color"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ColorB"))
            OnShapeChange()
        End Set
    End Property

    <XmlIgnore()> _
    Public Property Model() As RouteurModel
        Get
            Return _M
        End Get
        Set(ByVal value As RouteurModel)
            _M = value
        End Set

    End Property

    Private Sub OnShapeChange()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Shape"))
    End Sub

    Public Property RaceID() As String
        Get
            Return _RaceID
        End Get
        Set(ByVal value As String)
            _RaceID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceID"))
        End Set
    End Property

    Public Property RaceName() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceName"))
        End Set
    End Property

    Public Property Route() As ObservableCollection(Of RoutePointView)
        Get
            Return _Route
        End Get
        Set(ByVal value As ObservableCollection(Of RoutePointView))
            _Route = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Route"))
        End Set
    End Property

    Public Property RouteName() As String
        Get
            Return _RouteName
        End Get
        Set(ByVal value As String)
            _RouteName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteName"))
        End Set
    End Property

    <XmlIgnore()> _
   Public ReadOnly Property Shape() As Shapes.Path
        Get
            If Not Visible Then
                Return Nothing
            End If
            Dim Ret As New Shapes.Path
            Ret.Stroke = ColorBrush
            Ret.StrokeThickness = 1
            Dim sb As New StringBuilder

            For Each Pt In Route
                If sb.Length = 0 Then
                    sb.Append(" M ")
                Else
                    sb.Append(" L ")
                End If

                sb.Append(GetCoordsString(Model.The2DViewer.LonToCanvas(Pt.P.Lon_Deg), Model.The2DViewer.LatToCanvas(Pt.P.Lat_Deg)))

            Next

            Dim PC As New PathFigureCollectionConverter

            Dim PG As New PathGeometry
            PG.Figures = CType(PC.ConvertFromString(sb.ToString), PathFigureCollection)
            Ret.Data = PG

            Return Ret
        End Get
    End Property

    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get

        Set(ByVal value As Boolean)
            If Visible <> value Then
                _Visible = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Visible"))
                OnShapeChange()
            End If
        End Set

    End Property

End Class
