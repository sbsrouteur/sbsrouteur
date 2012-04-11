Imports System.ComponentModel

Public Class VLMBoatInfo
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _WPL As String  'liste de Waypoints (liste)
    Private _RAC As String  'numéro de la course (string)
    Private _IDB As String  'nom du bateau (string)
    Private _RAN As String  'nom de la course (string)
    Private _POS As String  'classement dans la course (string - xxx/yyy)
    Private _PIP As String  'pilot parameter (string - doit le rester à causes des WP: x.xx,y.yy
    Private _POL As String  'nom de la polaire (sans boat_) (string)
    Private _MCR As String  'mapCenter' (string), ie centre de la carte
    Private _MLY As String  'mapLayers' (string), ie type de layers
    Private _MOP As String  'mapOpponents' (string), ie type d'affichage des concurrents
    Private _MTL As String  'mapTools' (string), ie 
    Private _MPO As String  'mapPrefOpponents' (liste), ie concurrents à suivre
    Private _ETA As String  'Date estimée d'arrivé, seulement si pas de wp perso (string)
    Private _IDU As Integer 'numéro de bateau (int)
    Private _NWP As Integer  'numéro du prochain waypoing (int)
    Private _PIM As RoutePointView.EnumRouteMode
    Private _NUP As Integer  'nombre de secondes jusqu'à la prochaine VAC (int)
    Private _LUP As Integer  'Last update
    Private _MWD As String  'mapX' (int), ie taille largeur en pixel
    Private _MHT As String  'mapY' (int), ie taille hauteur en pixel
    Private _MAG As String  'mapAge' (int), ie age des trajectoires
    Private _MAR As String  'maparea' (int), ie taille de la carte
    Private _MES As String  'mapEstime' (int), ie estime
    Private _MGD As String  'mapMaille' (int), ie taille de la grid de vent
    Private _MDT As String  'mapDrawtextwp' (string) on/off
    Private _HDG As Double
    Private _DNM As Double  'Distance to next mark (float)
    Private _ORT As Double  'Cap ortho to next mark (float)
    Private _LOX As Double  'Cap loxo to next mark (float)
    Private _VMG As Double  'VMG (float)
    Private _TWD As Double  'Wind direction (float)
    Private _TWS As Double  'Wind speed (float)
    Private _TWA As Double  'Wind angle - Allure (float)
    Private _LOC As Double  'loch (float)
    Private _AVG As Double  'vitesse moyenne (float)
    Private _WPLAT As Double  'latitude du wp perso (float, en degré)
    Private _WPLON As Double  'longitude du wp perso (float, en degré)
    'private [_H@WP] as string  'mode Heading@WP, (float, degré)
    Private _LAT As Double  'latitude (float, degré)
    Private _LON As Double  'longitude (float, degré)
    Private _TUP As Integer  'Time to Update (à partir de NUP) (int)
    Private _TFS As Integer  'Time From Start (int)
    Private _RNK As Integer  'Rank as string  'classement dans la course (int)
    Private _NBS As String  'Number of Boat subscribed (int)
    Private _NPD As String  'Notepad (blocnote)
    Private _EML As String  'EMail
    Private _COL As String  'Color
    Private _CNT As String  'Country 
    Private _SRV As String  'Servername 
    Private _PIL1 As String ' Pilototo instruction 1 (id,time,PIM,PIP,status)
    Private _PIL2 As String ' Pilototo instruction 2 (id,time,PIM,PIP,status)
    Private _PIL3 As String ' Pilototo instruction 3 (id,time,PIM,PIP,status)
    Private _PIL4 As String ' Pilototo instruction 4 (id,time,PIM,PIP,status)
    Private _PIL5 As String ' Pilototo instruction 5 (id,time,PIM,PIP,status)
    Private _THM As String ' nom du theme
    Private _HID As String ' trace cachée (1) ou visible (0)
    Private _VAC As String ' durée de la vacation (en secondes)

    Private _Position As Coords

    ''' <summary>
    ''' 'vitesse du bateau (Boat SPeed) (float)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BSP() As Double


    Public ReadOnly Property [date] As DateTime
        Get
            Dim utc = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LUP)
            Return utc.ToLocalTime
        End Get

    End Property

    ''' <summary>
    ''' Position change since last update
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DIffClassement As Integer

    Public Property DNM() As Double
        Get
            Return _DNM
        End Get
        Set(ByVal value As Double)
            _DNM = value
            OnPropertyChanged("DNM")
        End Set
    End Property
    ''' <summary>
    ''' direction (HeaDinG)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HDG() As Double
        Get
            Return _HDG
        End Get
        Set(value As Double)
            _HDG = value
            OnPropertyChanged("HDG")
        End Set
    End Property

    Public Property IDU() As Integer
        Get
            Return _IDU
        End Get
        Set(ByVal value As Integer)
            _IDU = value
            OnPropertyChanged("IDU")
        End Set
    End Property

    Public Property LAT() As Double
        Get
            Return _LAT
        End Get
        Set(ByVal value As Double)
            _LAT = value
            OnPropertyChanged("LAT")
        End Set
    End Property
    Public Property LOC() As Double
        Get
            Return _LOC
        End Get
        Set(ByVal value As Double)
            _LOC = value
            OnPropertyChanged("LOC")
        End Set
    End Property
    Public Property LON() As Double
        Get
            Return _LON
        End Get
        Set(ByVal value As Double)
            _LON = value
            OnPropertyChanged("LON")
        End Set
    End Property
    Public Property LOX() As Double
        Get
            Return _LOX
        End Get
        Set(ByVal value As Double)
            _LOX = value
            OnPropertyChanged("LOX")
        End Set
    End Property
    Public Property LUP() As Integer
        Get
            Return _LUP
        End Get
        Set(ByVal value As Integer)
            _LUP = value
            OnPropertyChanged("LUP")
        End Set
    End Property
    Public Property NUP() As Integer
        Get
            Return _NUP
        End Get
        Set(ByVal value As Integer)
            _NUP = value
            OnPropertyChanged("NUP")
        End Set
    End Property
    Public Property NWP() As Integer
        Get
            Return _NWP
        End Get
        Set(ByVal value As Integer)
            _NWP = value
            OnPropertyChanged("NWP")
        End Set
    End Property

    Private Sub OnPropertyChanged(PropName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(PropName))
    End Sub

    Public Property ORT() As Double
        Get
            Return _ORT
        End Get
        Set(ByVal value As Double)
            _ORT = value
            OnPropertyChanged("ORT")
        End Set
    End Property
    Public Property PIL1() As String
        Get
            Return _PIL1
        End Get
        Set(ByVal value As String)
            _PIL1 = value
            OnPropertyChanged("PIL1")
        End Set
    End Property
    Public Property PIL2() As String
        Get
            Return _PIL2
        End Get
        Set(ByVal value As String)
            _PIL2 = value
            OnPropertyChanged("PIL2")
        End Set
    End Property
    Public Property PIL3() As String
        Get
            Return _PIL3
        End Get
        Set(ByVal value As String)
            _PIL3 = value
            OnPropertyChanged("PIL3")
        End Set
    End Property
    Public Property PIL4() As String
        Get
            Return _PIL4
        End Get
        Set(ByVal value As String)
            _PIL4 = value
            OnPropertyChanged("PIL4")
        End Set
    End Property
    Public Property PIL5() As String
        Get
            Return _PIL5
        End Get
        Set(ByVal value As String)
            _PIL5 = value
            OnPropertyChanged("PIL5")
        End Set
    End Property
    ''' <summary>
    ''' Pilot mode (int)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PIM() As RoutePointView.EnumRouteMode
        Get
            Return _PIM
        End Get
        Set(value As RoutePointView.EnumRouteMode)
            _PIM = value
            OnPropertyChanged("PIM")
            OnPropertyChanged("PIM_WP")
        End Set
    End Property

    Public ReadOnly Property PIM_WP As Boolean
        Get
            Return PIM >= RoutePointView.EnumRouteMode.Ortho
        End Get
    End Property
    Public Property PIP() As String
        Get
            Return _PIP
        End Get
        Set(ByVal value As String)
            _PIP = value
            OnPropertyChanged("PIP")
        End Set
    End Property

    ''' <summary>
    ''' 'nom de la polaire (sans boat_) (string)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POL() As String
        Get
            Return _POL
        End Get
        Set(value As String)
            _POL = value
            OnPropertyChanged("POL")
        End Set
    End Property

    Public Property POS() As String
        Get
            Return _POS
        End Get
        Set(value As String)
            _POS = value
            OnPropertyChanged("POS")
        End Set
    End Property

    Public Property Position As Coords
        Get
            If _Position Is Nothing Then
                _Position = New Coords(LAT, LON)
            End If
            Return _Position
        End Get
        Set(value As Coords)
            _Position = value
            OnPropertyChanged("Position")
        End Set
    End Property
    Public Property RNK() As Integer
        Get
            Return _RNK
        End Get
        Set(ByVal value As Integer)
            _RNK = value
            OnPropertyChanged("RNK")
        End Set
    End Property

    ''' <summary>
    ''' User track
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Track As String
        
    ''' <summary>
    '''  Time to Update (à partir de NUP) (int)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TUP() As Integer
        Get
            Return _TUP
        End Get
        Set(value As Integer)
            _TUP = value
            OnPropertyChanged("TUP")
        End Set
    End Property
    ''' <summary>
    ''' Wind angle - Allure (float)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TWA() As Double
        Get
            Return _TWA

        End Get
        Set(value As Double)
            _TWA = value
            OnPropertyChanged("TWA")
        End Set
    End Property

    Public Property TWD() As Double
        Get
            Return _TWD
        End Get
        Set(ByVal value As Double)
            _TWD = value
            OnPropertyChanged("TWD")
        End Set
    End Property

    Public Property TWS() As Double
        Get
            Return _TWS
        End Get
        Set(ByVal value As Double)
            _TWS = value
            OnPropertyChanged("TWS")
        End Set
    End Property


    Public Property VAC() As String
        Get
            Return _VAC
        End Get
        Set(ByVal value As String)
            _VAC = value
            OnPropertyChanged("VAC")
        End Set
    End Property
    Public Property VMG() As Double
        Get
            Return _VMG
        End Get
        Set(ByVal value As Double)
            _VMG = value
            OnPropertyChanged("VMG")
        End Set
    End Property
    ''' <summary>
    ''' latitude du wp perso (float, en degré)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property WPLAT() As Double
        Get
            Return _WPLAT
        End Get
        Set(value As Double)
            _WPLAT = value
            OnPropertyChanged("WPLAT")
        End Set
    End Property

    ''' <summary>
    ''' longitude du wp perso (float, en degré)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property WPLON() As Double
        Get
            Return _WPLON
        End Get
        Set(value As Double)
            _WPLON = value
            OnPropertyChanged("WPLON")
        End Set
    End Property


End Class
