using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/* Don't actually need this - would be nice to have, but don't need it, so don't bother

namespace VideoMusicSyncer.FFmpegCommandBuilder
{
	public class AbstractFilterGraph
	{
		//	public ImmutableHashSet<Node> Nodes { get; } = ImmutableHashSet.Create<Node>();
		//	//Would've used a HashSet<T> and wrapped it in a ReadOnlyCollection<T>
		//	//but for some reason ReadOnlyCollection<T> is actually a read-only-list
		//	//and so requires an IList<T>, which HashSet<T> doesn't implement

		public Node Output { get; } //Might need multiple outputs

		public AbstractFilterGraph(FFmpegFilter originFilter)
		{
			this.Output = new Node(this, new List<ForwardLink>());
		}

		private void AddNode(Node node)
		{

		}

		public sealed class Node
		{
			public AbstractFilterGraph Graph { get; }

			public List<ForwardLink> Links { get; }

			public Node(AbstractFilterGraph graph, List<ForwardLink> links)
			{
				ErrorUtils.ThrowIfArgNull(graph, nameof(graph));
				ErrorUtils.ThrowIfArgNull(links, nameof(links));

				this.Graph = graph;
				this.Links = links;

				this.Graph.AddNode(this);
			}
		}

		public struct ForwardLink
		{
			public FFmpegFilter Filter { get; }
			public Node Target { get; }

			public ForwardLink(FFmpegFilter filter, Node target)
			{
				ErrorUtils.ThrowIfArgNull(filter, nameof(filter));
				ErrorUtils.ThrowIfArgNull(target, nameof(target));

				this.Filter = filter;
				this.Target = target;
			}
		}

		//	private class PrivateNode : Node
		//	{
		//		public PrivateNode(List<Node> inputPads, FFmpegFilter filter, List<Node> outputPads)
		//			: base(inputPads, filter, outputPads)
		//		{ }
		//	}
	}
}

//*/