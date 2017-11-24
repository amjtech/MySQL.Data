﻿// Copyright © 2015, 2017, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using MySqlX.Protocol.X;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.CRUD;
using System;
using System.Net.Sockets;
using Xunit;

namespace MySqlX.Data.Tests
{
  public class BasicFindTests : BaseTest
  {
    [Fact]
    public void SimpleFind()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      DocResult foundDocs = coll.Find("pages > 20").Execute();
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 2");
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 3");
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 4");
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void SimpleFindWithSort()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      DocResult foundDocs = coll.Find("pages > 20").OrderBy("pages DESC").Execute();
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 4");
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 3");
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 2");
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void SimpleFindWithLimit()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      DocResult foundDocs = coll.Find("pages > 20").Limit(1).Execute();
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 2");
      Assert.False(foundDocs.Next());

      // Limit out of range.
      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Find().Limit(0).Execute());
      Assert.Throws<ArgumentOutOfRangeException>(() => coll.Find().Limit(-1).Execute());
    }

    [Fact]
    public void FindConditional()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      DocResult foundDocs = coll.Find("pages = :Pages").Bind("pAges", 40).Execute();
      Assert.True(foundDocs.Next());
      Assert.True(foundDocs.Current["title"] == "Book 3");
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void BindDbDoc()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      //var s = MySql.Data.ResourcesX.TestingResources;

      DbDoc docParams = new DbDoc(new { pages1 = 30, pages2 = 40 });
      DocResult foundDocs = coll.Find("pages = :Pages1 || pages = :Pages2").Bind(docParams).Execute();
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 2", foundDocs.Current["title"]);
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 3", foundDocs.Current["title"]);
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void BindJsonAsAnonymous()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      var jsonParams = new { pages1 = 30, pages2 = 40 };
      DocResult foundDocs = coll.Find("pages = :Pages1 || pages = :Pages2").Bind(jsonParams).Execute();
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 2", foundDocs.Current["title"]);
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 3", foundDocs.Current["title"]);
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void BindJsonAsString()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      var jsonParams = "{ \"pages1\" : 30, \"pages2\" : 40 }";
      DocResult foundDocs = coll.Find("pages = :Pages1 || pages = :Pages2").Bind(jsonParams).Execute();
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 2", foundDocs.Current["title"]);
      Assert.True(foundDocs.Next());
      Assert.Equal("Book 3", foundDocs.Current["title"]);
      Assert.False(foundDocs.Next());
    }

    [Fact]
    public void RowLockingNotSupportedInOlderVersions()
    {
      if (session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      Collection coll = CreateCollection("test");

      Exception ex = Assert.Throws<MySqlException>(() => coll.Find().LockShared().Execute());
      Assert.Equal("This functionality is only supported from server version 8.0.3 onwards.", ex.Message);

      ex = Assert.Throws<MySqlException>(() => coll.Find().LockExclusive().Execute());
      Assert.Equal("This functionality is only supported from server version 8.0.3 onwards.", ex.Message);
    }

    [Fact]
    public void SimpleSharedLock()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Should return immediately since document isn't locked.
        docResult = coll2.Find("_id = 2").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        // Should return immediately due to LockShared() allows reading by other sessions.
        docResult = coll2.Find("_id = 1").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session.SQL("ROLLBACK").Execute();
        session2.SQL("ROLLBACK").Execute();
      }
    }

    [Fact]
    public void SimpleExclusiveLock()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        coll.CreateIndex("myIndex", true).Field("$._id", "INT", true).Execute();
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Should return immediately since document isn't locked.
        docResult = coll2.Find("_id = 2").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        // Session2 blocks due to to LockExclusive() not allowing to read locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Find("_id = 1").LockExclusive().Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        session.SQL("ROLLBACK").Execute();
        session2.SQL("ROLLBACK").Execute();
      }
    }

    [Fact]
    public void SharedLockForbidsToModifyDocuments()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Reading the same document is allowed with LockShared().
        docResult = coll2.Find("_id = 1").Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        // Modify() is allowed for non-locked documents.
        Result result = coll2.Modify("_id = 2").Set("a", 2).Execute();
        Assert.Equal<ulong>(1, result.RecordsAffected);
        // Session1 blocks, Modify() is not allowed for locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Modify("_id = 1").Set("a", 2).Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        session.SQL("ROLLBACK").Execute();
        // Modify() is allowed since document isn't locked anymore.
        coll2.Modify("_id = 1").Set("a", 2).Execute();
        session2.SQL("COMMIT").Execute();
      }
    }

    [Fact]
    public void ExclusiveLockForbidsToModifyDocuments()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();

        // Modify() is allowed for non-locked documents.
        Result result = coll2.Modify("_id = 2").Set("a", 2).Execute();
        Assert.Equal<ulong>(1, result.RecordsAffected);
        // Session1 blocks, Modify() is not allowed for locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Modify("_id = 1").Set("a", 2).Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        session.SQL("ROLLBACK").Execute();
        // Modify() is allowed since document isn't locked anymore.
        coll2.Modify("_id = 1").Set("a", 2).Execute();
        session2.SQL("COMMIT").Execute();
      }
    }

    [Fact]
    public void SharedLockAfterExclusiveLock()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        coll.CreateIndex("myIndex", true).Field("$._id", "INT", true).Execute();
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Should return immediately since document isn't locked.
        docResult = coll2.Find("_id = 2").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        // Session2 blocks due to LockExclusive() not allowing to read locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Find("_id = 1").LockShared().Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        // Session unlocks documents.
        session.SQL("ROLLBACK").Execute();
        // Document can now be recovered.
        docResult = coll2.Find("_id = 1").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        session2.SQL("ROLLBACK").Execute();
      }
    }

    [Fact]
    public void ExclusiveLockAfterSharedLock()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        coll.CreateIndex("myIndex", true).Field("$._id", "INT", true).Execute();
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 },
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id in (1, 3)").LockShared().Execute();
        Assert.Equal(2, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Should return immediately since document isn't locked.
        docResult = coll2.Find("_id = 2").LockExclusive().Execute();
        // Should return immediately due to LockShared() allows reading by other sessions.
        docResult = coll2.Find("_id = 2").LockShared().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        // Session2 blocks due to to LockExclusive() not allowing to read locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Find("_id = 1").LockExclusive().Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        // Session unlocks documents.
        session.SQL("ROLLBACK").Execute();
        docResult = coll2.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        session2.SQL("ROLLBACK").Execute();
      }
    }

    [Fact]
    public void ExclusiveLockAfterExclusiveLock()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      session.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
      using (var session2 = MySQLX.GetSession(ConnectionString))
      {
        session2.SQL("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED").Execute();
        Collection coll = CreateCollection("test");
        coll.CreateIndex("myIndex", true).Field("$._id", "INT", true).Execute();
        var docs = new[]
        {
          new {  _id = 1, a = 1 },
          new {  _id = 2, a = 1 },
          new {  _id = 3, a = 1 }
        };
        coll.Add(docs).Execute();
        Collection coll2 = session2.GetSchema("test").GetCollection("test");

        session.SQL("START TRANSACTION").Execute();
        DocResult docResult = coll.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);

        session2.SQL("START TRANSACTION").Execute();
        // Should return immediately since document isn't locked.
        docResult = coll2.Find("_id = 2").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        // Session2 blocks due to to LockExclusive() not allowing to read locked documents.
        session2.SQL("SET SESSION innodb_lock_wait_timeout=1").Execute();
        Exception ex = Assert.Throws<MySqlException>(() => coll2.Find("_id = 1").LockExclusive().Execute());
        Assert.Equal("Lock wait timeout exceeded; try restarting transaction", ex.Message);

        // Session unlocks documents.
        session.SQL("ROLLBACK").Execute();
        docResult = coll2.Find("_id = 1").LockExclusive().Execute();
        Assert.Equal(1, docResult.FetchAll().Count);
        session2.SQL("ROLLBACK").Execute();
      }
    }

    [Fact]
    public void InOperatorWithListOfValues()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      // Validates the IN operator allows expressions of the type
      // ( compExpr ["NOT"] "IN" "(" argsList ")" ) | ( compExpr ["NOT"] "IN" "[" argsList "]" )
      Collection coll = CreateCollection("test");
      coll.Add(new DbDoc("{ \"a\": 1, \"b\": [ 1, \"value\" ], \"d\":\"\" }")).Execute();

      Assert.Equal(1, coll.Find("a IN (1,2,3)").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("a not in (0,2,3)").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("b[0] in (1,2,3)").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("b[1] in (\"a\", \"b\", \"value\")").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("b[0] NOT IN (0,2,3)").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("b[1] not in (\"a\", \"b\", \"c\")").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("a in [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("a in [2,3,4]").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("a NOT in [0,2,3]").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("b not IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("b[0] not IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("c NOT IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("a IN ('', ' ')").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("'' IN (1,2,3)").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("d IN ('')").Execute().FetchAll().Count);

      Collection movies = CreateCollection("movies");
      var docString = "{ \"_id\" : \"a6f4b93e1a264a108393524f29546a8c\", \"title\" : \"AFRICAN EGG\", \"description\" : \"A Fast-Paced Documentary of a Pastry Chef And a Dentist who must Pursue a Forensic Psychologist in The Gulf of Mexico\", \"releaseyear\" : 2006, \"language\" : \"English\", \"duration\" : 130, \"rating\" : \"G\", \"genre\" : \"Science fiction\", \"actors\" : [{ \"name\" : \"MILLA PECK\", \"country\" : \"Mexico\", \"birthdate\": \"12 Jan 1984\"}, { \"name\" : \"VAL BOLGER\", \"country\" : \"Botswana\", \"birthdate\": \"26 Jul 1975\" }, { \"name\" : \"SCARLETT BENING\", \"country\" : \"Syria\", \"birthdate\": \"16 Mar 1978\" }], \"additionalinfo\" : { \"director\" : \"Sharice Legaspi\", \"writers\" : [\"Rusty Couturier\", \"Angelic Orduno\", \"Carin Postell\"], \"productioncompanies\" : [\"Qvodrill\", \"Indigoholdings\"] } }";
      movies.Add(new DbDoc(docString)).Execute();

      Assert.Equal(1, movies.Find("(1>5) in (true, false)").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("(1+5) in (1, 2, 3, 4, 5)").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("('a'>'b') in (true, false)").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("(1>5) in [true, false]").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("(1+5) in [1, 2, 3, 4, 5]").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("('a'>'b') in [true, false]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("true IN [(1>5), !(false), (true || false), (false && true)]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("true IN ((1>5), !(false), (true || false), (false && true))").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("{\"field\":true} IN (\"mystring\", 124, myvar, othervar.jsonobj)").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("actor.name IN ['a name', null, (1<5-4), myvar.jsonobj.name]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("!false && true IN [true]").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("1-5/2*2 > 3-2/1*2 IN [true, false]").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("true IN [1-5/2*2 > 3-2/1*2]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find(" 'African Egg' IN ('African Egg', 1, true, NULL, [0,1,2], { 'title' : 'Atomic Firefighter' }) ").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find(" 1 IN ('African Egg', 1, true, NULL, [0,1,2], { 'title' : 'Atomic Firefighter' }) ").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find(" [0,1,2] IN ('African Egg', 1, true, NULL, [0,1,2], { 'title' : 'Atomic Firefighter' }) ").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find(" { 'title' : 'Atomic Firefighter' } IN ('African Egg', 1, true, NULL, [0,1,2], { 'title' : 'Atomic Firefighter' }) ").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("title IN ('African Egg', 'The Witcher', 'Jurassic Perk')").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("releaseyear IN (2006, 2010, 2017)").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("1 IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("0 IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("0 NOT IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("1 NOT IN [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("releaseyear IN [2006, 2007, 2008]").Execute().FetchAll().Count);
    }

    [Fact]
    public void InOperatorWithCompExpr()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      // Validates the IN operator allows expressions of the type: compExpr ["NOT"] "IN" compExpr
      Collection coll = CreateCollection("test");
      var docString = "{ \"a\": 1, \"b\": \"foo\", \"c\": { \"d\": true, \"e\": [1,2,3] }, \"f\": [ {\"x\":5}, {\"x\":7 } ] }";
      coll.Add(new DbDoc(docString)).Execute();

      Assert.Equal(1, coll.Find("a in [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("c.e[0] in [1,2,3]").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("5 in f[*].x").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("3 in c.e").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("5 in c.e").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("\"foo\" in " + docString).Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("\"a\" in " + docString).Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("a in " + docString).Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("{\"a\":1} in " + docString).Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("\"foo\" in b").Execute().FetchAll().Count);

      Collection movies = CreateCollection("movies");
      docString = "{ \"_id\" : \"a6f4b93e1a264a108393524f29546a8c\", \"title\" : \"AFRICAN EGG\", \"description\" : \"A Fast-Paced Documentary of a Pastry Chef And a Dentist who must Pursue a Forensic Psychologist in The Gulf of Mexico\", \"releaseyear\" : 2006, \"language\" : \"English\", \"duration\" : 130, \"rating\" : \"G\", \"genre\" : \"Science fiction\", \"actors\" : [{ \"name\" : \"MILLA PECK\", \"country\" : \"Mexico\", \"birthdate\": \"12 Jan 1984\"}, { \"name\" : \"VAL BOLGER\", \"country\" : \"Botswana\", \"birthdate\": \"26 Jul 1975\" }, { \"name\" : \"SCARLETT BENING\", \"country\" : \"Syria\", \"birthdate\": \"16 Mar 1978\" }], \"additionalinfo\" : { \"director\" : \"Sharice Legaspi\", \"writers\" : [\"Rusty Couturier\", \"Angelic Orduno\", \"Carin Postell\"], \"productioncompanies\" : [\"Qvodrill\", \"Indigoholdings\"] } }";
      movies.Add(new DbDoc(docString)).Execute();

      Assert.Equal(1, movies.Find("{ \"name\" : \"MILLA PECK\" } IN actors").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("'African Egg' in movietitle").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("(1 = NULL) IN title").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => movies.Find("NOT NULL IN title").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("[\"Rusty Couturier\", \"Angelic Orduno\", \"Carin Postell\"] IN additionalinfo.writers").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("{ \"name\" : \"MILLA PECK\", \"country\" : \"Mexico\", \"birthdate\": \"12 Jan 1984\"} IN actors").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("true IN title").Execute().FetchAll().Count);
      Assert.Equal(0, movies.Find("false IN genre").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("'Sharice Legaspi' IN additionalinfo.director").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("'Mexico' IN actors[*].country").Execute().FetchAll().Count);
      Assert.Equal(1, movies.Find("'Angelic Orduno' IN additionalinfo.writers").Execute().FetchAll().Count);
    }

    [Fact]
    public void InOperatorWithJsonArrays()
    {
      if (!session.InternalSession.GetServerVersion().isAtLeast(8,0,3)) return;

      Collection coll = CreateCollection("test");
      var docString = "{ \"_id\": \"1001\", \"ARR\":[1,2,3], \"ARR1\":[\"name\", \"name2\", \"name3\"]}";
      coll.Add(new DbDoc(docString)).Execute();

      Assert.Equal(1, coll.Find("\"1001\" in $._id").Execute().FetchAll().Count);
      Assert.Equal(0, coll.Find("\"1002\" in $._id").Execute().FetchAll().Count);
      Assert.Equal(1, coll.Find("(1+2) in (1, 2, 3)").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => coll.Find("(1+2) in [1, 2, 3]").Execute().FetchAll().Count);
      Assert.Throws<MySqlException>(() => coll.Find("(1+2) in $.ARR").Execute().FetchAll().Count);
    }
    [Fact]
    public void GetOne()
    {
      Collection coll = CreateCollection("test");
      var docs = new[]
      {
        new {  _id = 1, title = "Book 1", pages = 20 },
        new {  _id = 2, title = "Book 2", pages = 30 },
        new {  _id = 3, title = "Book 3", pages = 40 },
        new {  _id = 4, title = "Book 4", pages = 50 },
      };
      Result r = coll.Add(docs).Execute();
      Assert.Equal<ulong>(4, r.RecordsAffected);

      // Expected exceptions.
      Assert.Throws<ArgumentNullException>(() => coll.GetOne(null));
      Assert.Throws<ArgumentNullException>(() => coll.GetOne(""));
      Assert.Throws<ArgumentNullException>(() => coll.GetOne(string.Empty));

      // Get document using numeric parameter.
      DbDoc document = coll.GetOne(1);
      Assert.Equal(1, document.Id);
      Assert.Equal("Book 1", document["title"]);
      Assert.Equal(20, Convert.ToInt32(document["pages"]));

      // Get document using string parameter.
      document = coll.GetOne("3");
      Assert.Equal(3, document.Id);
      Assert.Equal("Book 3", document["title"]);
      Assert.Equal(40, Convert.ToInt32(document["pages"]));

      // Get a non-existing document.
      document = coll.GetOne(5);
      Assert.Equal(null, document);
    }
  }
}

