﻿using Akka.TestKit.NUnit3;
using DepthFirstSearchOfATree.AkkaDotNetExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit;
using NUnit.Framework;

namespace DepthFirstSearchOfATree.Tests
{

    public class NodeActorFactoryTest : TestKit, INodeActorFactory
    {
        public string NodeName { get; }

        public TestProbe Probe { get; }

        public NodeActorFactoryTest(string nodeName)
        {
            NodeName = nodeName;
            Probe = CreateTestProbe(nodeName);
        }

        public IActorRef Create(IActorRefFactory refFactory)
        {
            return Probe;
        }
    }

    [TestFixture]
    public class ConcurencyTest : TestKit
    {

        #region tests_beginning_with_add_request
        [Test]
        public void Probe_should_receive_add_request()
        {
            var rootFactory = new NodeActorFactoryTest("root");
            var child1Factory = new NodeActorFactoryTest("child1");

            var tree = Sys.ActorOf(Props.Create(() => new TreeActor(rootFactory)), "tree");

            tree.Tell(new NodeActor.AddRequest(child1Factory, "root", tree));

            var rootProbe = rootFactory.Probe;

            rootProbe.ExpectMsg<NodeActor.AddRequest>();
        }

        [Test]
        public void Probe_should_not_received_add_request_after_add_request()
        {
            var rootFactory = new NodeActorFactoryTest("root");
            var child1Factory = new NodeActorFactoryTest("child1");

            var tree = Sys.ActorOf(Props.Create(() => new TreeActor(rootFactory)), "tree");

            tree.Tell(new NodeActor.AddRequest(child1Factory, "root", tree));
            tree.Tell(new NodeActor.VisitRequest());

            var rootProbe = rootFactory.Probe;

            rootProbe.ExpectMsg<NodeActor.AddRequest>();
            rootProbe.ExpectNoMsg();
        }

        [Test]
        public void Probe_should_stop_receiving_any_requests_after_add_request()
        {
            var rootFactory = new NodeActorFactoryTest("root");
            var child1Factory = new NodeActorFactoryTest("child1");
            var child2Factory = new NodeActorFactoryTest("child2");
            var child3Factory = new NodeActorFactoryTest("child3");
            var child4Factory = new NodeActorFactoryTest("child4");

            var tree = Sys.ActorOf(Props.Create(() => new TreeActor(rootFactory)), "tree");

            tree.Tell(new NodeActor.AddRequest(child1Factory, "root", tree));
            tree.Tell(new NodeActor.VisitRequest());
            tree.Tell(new NodeActor.AddRequest(child2Factory, "root", tree));
            tree.Tell(new NodeActor.AddRequest(child3Factory, "root", tree));
            tree.Tell(new NodeActor.VisitRequest());
            tree.Tell(new NodeActor.AddRequest(child4Factory, "root", tree));

            var rootProbe = rootFactory.Probe;

            rootProbe.ExpectMsg<NodeActor.AddRequest>();
            rootProbe.ExpectNoMsg();
        }

        [Test]
        public void Probe_should_received_add_request_after_add_request()
        {
            var rootFactory = new NodeActorFactoryTest("root");
            var child1Factory = new NodeActorFactoryTest("child1");
            var child2Factory = new NodeActorFactoryTest("child2");

            var tree = Sys.ActorOf(Props.Create(() => new TreeActor(rootFactory)), "tree");

 
            tree.Tell(new NodeActor.AddRequest(rootFactory, "root", tree));
            tree.Tell(new NodeActor.VisitRequest());

            var rootProbe = rootFactory.Probe;

            rootProbe.ExpectMsg<NodeActor.AddRequest>(m =>
            {
                tree.Tell(new NodeActor.AddResult());

                rootProbe.ExpectMsg<NodeActor.VisitRequest>();
            });
        }

        [Test]
        public void Probe_should_receive_all_requests_after_add_request()
        {
            var rootFactory = new NodeActorFactoryTest("root");
            var child1Factory = new NodeActorFactoryTest("child1");
            var child2Factory = new NodeActorFactoryTest("child2");
            var child3Factory = new NodeActorFactoryTest("child3");

            var tree = Sys.ActorOf(Props.Create(() => new TreeActor(rootFactory)), "tree");

            tree.Tell(new NodeActor.AddRequest(child1Factory, "root", tree));
            tree.Tell(new NodeActor.VisitRequest());
            tree.Tell(new NodeActor.AddRequest(child2Factory, "root", tree));
            tree.Tell(new NodeActor.AddRequest(child3Factory, "root", tree));

            var rootProbe = rootFactory.Probe;

            rootProbe.ExpectMsg<NodeActor.AddRequest>(m1 =>
            {
                tree.Tell(new NodeActor.AddResult());

                rootProbe.ExpectMsg<NodeActor.VisitRequest>(m2 =>
                {
                    tree.Tell(new NodeActor.VisitResult(m2));

                    rootProbe.ExpectMsg<NodeActor.AddRequest>();
                    rootProbe.ExpectMsg<NodeActor.AddRequest>();
                });
            });
        }
        #endregion tests_beginning_with_add_request
    }
}