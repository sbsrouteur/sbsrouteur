'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Public Class VLMBoatRanking

    Private _idusers As Integer
    Private _boatpseudo As String
    Private _boatname As String
    Private _color As String
    Private _country As String
    Private _nwp As Integer
    Private _dnm As Double
    Private _deptime As Long
    Private _loch As Double
    Private _releasetime As Long
    Private _latitude As Double
    Private _longitude As Double
    Private _last1h As Double
    Private _last3h As Double
    Private _last24h As Double
    Private _status As String
    Private _rank As Integer

    Public Property boatname() As String
        Get
            Return _boatname
        End Get
        Set(ByVal value As String)
            _boatname = value
        End Set
    End Property

    Public Property boatpseudo() As String
        Get
            Return _boatpseudo
        End Get
        Set(ByVal value As String)
            _boatpseudo = value
        End Set
    End Property

    Public Property color() As String
        Get
            Return _color
        End Get
        Set(ByVal value As String)
            _color = value
        End Set
    End Property

    Public Property country() As String
        Get
            Return _country
        End Get
        Set(ByVal value As String)
            _country = value
        End Set
    End Property

    Public Property deptime() As Long
        Get
            Return _deptime
        End Get
        Set(ByVal value As Long)
            _deptime = value
        End Set
    End Property

    Public Property dnm() As Double
        Get
            Return _dnm
        End Get
        Set(ByVal value As Double)
            _dnm = value
        End Set
    End Property

    Public Property idusers() As Integer
        Get
            Return _idusers
        End Get
        Set(ByVal value As Integer)
            _idusers = value
        End Set
    End Property

    Public Property last1h() As Double
        Get
            Return _last1h
        End Get
        Set(ByVal value As Double)
            _last1h = value
        End Set
    End Property

    Public Property last24h() As Double
        Get
            Return _last24h
        End Get
        Set(ByVal value As Double)
            _last24h = value
        End Set
    End Property

    Public Property last3h() As Double
        Get
            Return _last3h
        End Get
        Set(ByVal value As Double)
            _last3h = value
        End Set
    End Property

    Public Property latitude() As Double
        Get
            Return _latitude
        End Get
        Set(ByVal value As Double)
            _latitude = value
        End Set
    End Property

    Public Property loch() As Double
        Get
            Return _loch
        End Get
        Set(ByVal value As Double)
            _loch = value
        End Set
    End Property

    Public Property longitude() As Double
        Get
            Return _longitude
        End Get
        Set(ByVal value As Double)
            _longitude = value
        End Set
    End Property

    Public Property nwp() As Integer
        Get
            Return _nwp
        End Get
        Set(ByVal value As Integer)
            _nwp = value
        End Set
    End Property

    Public Property rank() As Integer
        Get
            Return _rank
        End Get
        Set(ByVal value As Integer)
            _rank = value
        End Set
    End Property

    Public Property releasetime() As Long
        Get
            Return _releasetime
        End Get
        Set(ByVal value As Long)
            _releasetime = value
        End Set
    End Property

    Public Property status() As String
        Get
            Return _status
        End Get
        Set(ByVal value As String)
            _status = value
        End Set
    End Property

End Class
