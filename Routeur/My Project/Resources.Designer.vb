﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Ce code a été généré par un outil.
'     Version du runtime :4.0.30319.18444
'
'     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
'     le code est régénéré.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    'à l'aide d'un outil, tel que ResGen ou Visual Studio.
    'Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    'avec l'option /str ou régénérez votre projet VS.
    '''<summary>
    '''  Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Routeur.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Remplace la propriété CurrentUICulture du thread actuel pour toutes
        '''  les recherches de ressources à l'aide de cette classe de ressource fortement typée.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Recherche une chaîne localisée semblable à CREATE TABLE [DBVersion] (
        '''[VersionNumber] iNTEGER  UNIQUE NOT NULL PRIMARY KEY,
        '''[VersionDate] timesTAMP DEFAULT CURRENT_TIMESTAMP UNIQUE NOT NULL
        ''');
        '''
        '''CREATE TABLE [MapsSegments] (
        '''[IdSegment] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
        '''[MapLevel] iNTEGER  NOT NULL,
        '''[Lon1] reAL  NOT NULL,
        '''[lat1] rEAL  NOT NULL,
        '''[lon2] rEAL  NOT NULL,
        '''[lat2] rEAL  NOT NULL
        ''');
        '''
        '''CREATE INDEX [IdxSegments] ON [MapsSegments](
        '''[MapLevel]  ASC,
        '''[Lon1]  ASC,
        '''[lat1]  ASC
        ''');
        '''
        '''insert into DBVersion(VersionNumber)  [le reste de la chaîne a été tronqué]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateDBScript() As String
            Get
                Return ResourceManager.GetString("CreateDBScript", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Recherche une chaîne localisée semblable à CREATE VIRTUAL TABLE MapLevel_Idx1 USING rtree(
        '''   id,              -- Integer primary key
        '''   minX, maxX,      -- Minimum and maximum X coordinate
        '''   minY, maxY       -- Minimum and maximum Y coordinate
        ''');
        '''CREATE VIRTUAL TABLE MapLevel_Idx2 USING rtree(
        '''   id,              -- Integer primary key
        '''   minX, maxX,      -- Minimum and maximum X coordinate
        '''   minY, maxY       -- Minimum and maximum Y coordinate
        ''');
        '''CREATE VIRTUAL TABLE MapLevel_Idx3 USING rtree(
        '''   id,              -- Integer primary ke [le reste de la chaîne a été tronqué]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateRangeIndex() As String
            Get
                Return ResourceManager.GetString("CreateRangeIndex", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Recherche une chaîne localisée semblable à begin transaction;
        '''
        '''insert into DBVersion (VersionNumber) Values (2);
        '''
        '''CREATE TABLE `Tracks`
        '''(
        '''       ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
        '''       RaceId INTEGER NOT NULL,
        '''       BoatNum INTEGER NOT NULL
        ''');
        '''
        '''CREATE TABLE `TrackPoints`
        '''(
        '''       RefTrack INTEGER NOT NULL,
        '''       IdPoint INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
        '''       PointDate DATETIME NOT NULL,
        '''       Lon FLOAT NOT NULL,
        '''       Lat FLOAT NOT NULL
        ''');
        '''
        '''commit transaction;.
        '''</summary>
        Friend ReadOnly Property UpdateV1ToV2() As String
            Get
                Return ResourceManager.GetString("UpdateV1ToV2", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Recherche une chaîne localisée semblable à begin transaction;
        '''insert into DBVersion (VersionNumber) Values (3);
        '''drop table TrackPoints;
        '''CREATE TABLE `TrackPoints`
        '''(
        '''       RefTrack INTEGER NOT NULL,
        '''       IdPoint INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
        '''       PointDate INTEGER NOT NULL,
        '''       Lon FLOAT NOT NULL,
        '''       Lat FLOAT NOT NULL
        ''');
        '''CREATE UNIQUE INDEX IdxTrackPoints
        ''' ON `TrackPoints` (RefTrack,PointDate);
        '''Commit transaction;.
        '''</summary>
        Friend ReadOnly Property UpdateV2ToV3() As String
            Get
                Return ResourceManager.GetString("UpdateV2ToV3", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Recherche une chaîne localisée semblable à begin transaction;
        '''insert into DBVersion (VersionNumber) Values (4);
        '''Analyze;
        '''Commit Transaction;.
        '''</summary>
        Friend ReadOnly Property UpdateV3ToV4() As String
            Get
                Return ResourceManager.GetString("UpdateV3ToV4", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
