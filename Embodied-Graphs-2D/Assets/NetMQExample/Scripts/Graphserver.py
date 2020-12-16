#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq

import argparse, json, os
import subprocess

def str2bool(v):
    if isinstance(v, bool):
        return v
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Boolean value expected.')

parser = argparse.ArgumentParser()
#https://stackoverflow.com/a/43357954
parser.add_argument('--graph_to_hypergraph', type=str2bool, nargs='?', const=True, default=False, help= "convert a graph to simplicial.")
parser.add_argument('--graph_to_simplicial', type=str2bool, nargs='?', const=True, default=False, help ="convert a graph to hypergraph")
parser.add_argument('--simplicial_to_graph', type=str2bool, nargs='?', const=True, default=False, help = "convert a simplicial to graph")
parser.add_argument('--simplicial_to_hypergraph', type=str2bool, nargs='?', const=True, default=False, help ="convert a simplicial to hypergraph.")
parser.add_argument('--hypergraph_to_graph', type=str2bool, nargs='?', const=True, default=False, help ="convert a hypergraph to graph.")
parser.add_argument('--hypergraph_to_simplicial', type=str2bool, nargs='?', const=True, default=False, help ="convert a hypergraph to simplicial.")

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:8080")

# format: {<nodes seperated by comma>-{edge_1}{edge_2}....}
def parse_message(sent_Str):
    sent_Str=sent_Str.split("-")
    print(sent_Str)

    node_Str=sent_Str[0][1:-1]
    print(node_Str)

    edge_Str=sent_Str[-1].split("}")
    print(edge_Str)

    nodes_list = []
    for each_node in node_Str.split(','):
        nodes_list.append(int(each_node))
    print(nodes_list)    

    edges_list = []
    for each_edge_Str in edge_Str:
        if each_edge_Str == '':
            continue
        each_edge_Str = each_edge_Str[1:]
        cur_edge = []
        for edge_node in each_edge_Str.split(','):
            cur_edge.append(int(edge_node))
        print(cur_edge)    
        edges_list.append(frozenset(cur_edge))

    print(edges_list)    
    return nodes_list, edges_list
 

from itertools import combinations

## So we can have an empty edge/face/simplex set everywhere, even the
## node set can be empty
## TODO: 
## Understand whether simplicial complex constrction from graph has
## some complex procudeure by looking at neighborhood and cliques
## Then try to understand if we really need point simplices in the
## simplicial complex. If the is really required, do we need to put
## edges less than 2 nodes in hypergraph?
## Another question, do we create a point simplex if it just a dangling node
## in simplicial complex?

## Node and edge subtraction
## So if we remove triangle we just keep the traingle but keeps the edges

class Graph:
    """
    A Graph is a tuple (nodes,edges) where the element of nodes are called nodes
    or vertices and each element e of edges are called edge where |e| =2
    for all e.
    """
    def __init__(self, nodes=None, edges=None):
        self.nodes = set()
        self.edges = set()
        if nodes:
            for node in nodes:
                self.add_node(node)
        if edges:
            for edge in edges:
                self.add_edge(edge)


    def add_node(self,node):
        self.nodes.add(node)

    def add_edge(self,edge):
        """
        Add an edge to the set of edges
        """
        self.edges.add(frozenset(edge))
        for node in edge:
            self.add_node(node)

    def to_simplicial_complex(self):
        pass
        nodes = self.nodes
        simplices = self.edges
        return(SimplicialComplex(nodes=nodes, simplices=simplices))

    def to_hypergraph(self):
        pass
        nodes = self.nodes
        hyperedges = self.edges
        return(HyperGraph(nodes=nodes, hyperedges=hyperedges))


def get_subsets(s, n): 
    return [frozenset(i) for i in combinations(s, n)] 

def frozen_set_to_list(frozenlist):
    sets=list(frozenlist)
    edges = [list(x) for x in sets]
    return edges

class SimplicialComplex:
    """
    A simplicial complex is a tuple (V, S) where the elements of V are
    called nodes or vertices, and each element s of S is called a simplex.
    Each s is a subset of V. Downward inclusion property of simplicial
    complex dictates that if s is a subset of S and r is a subset of s
    then r must be inside S

    """
    def __init__(self, nodes=None, simplices=None):
        self.nodes = set()
        self.simplices = set()
        if nodes:
            for node in nodes:
                self.add_node(node)
        if simplices:
            ## TODO add combinations
            for simplex in simplices:
                self.add_simplex(simplex)

    def add_node(self,node):
        self.nodes.add(node)

    def add_simplex(self,simplex):
        """
        Add a simplex to the simplicial complex
        """
        self.simplices.add(frozenset(simplex))
        ## To make sure the downward inclusion
        ## we are adding all the subsets of the simplex
        ## in the simplces set
        for i in range(1,len(simplex)):
        ## We do not want any simplex that is empty and
        ## we have already put the whole simplex itself in
        ## the line outside the loop
            self.simplices.update(get_subsets(simplex,i))
        for node in simplex:
            self.add_node(node)

    def to_hypergraph(self):
        ## TODO need to make sure whether 1 simplices
        ## or the point simplices should be in hyper graph or not
        pass
        nodes = self.nodes
        hyperedges = [e for e in self.simplices if len(e)>1]
        return HyperGraph(nodes=nodes,hyperedges=hyperedges)

    def to_graph(self):
        pass
        nodes = self.nodes
        edges = []
        for simplex in self.simplices:
            if len(simplex) < 2:
                pass
                ## We are not keeping the point simplices
            elif len(simplex) == 2:
                edges.append(simplex)
            else:
                ## We are creating 2 length subsets to 
                ## the edge set
                edges.extend(get_subsets(simplex,2))
        return Graph(nodes=nodes,edges=edges)

class HyperGraph:
    def __init__(self, nodes=None, hyperedges=None):
        self.nodes = set()
        self.hyperedges = set()
        if nodes:
            for node in nodes:
                self.add_node(node)
        if hyperedges:
            for hyperedge in hyperedges:
                #print(hyperedge)
                self.add_hyperedge(hyperedge)

    def add_node(self,node):
        self.nodes.add(node)

    def add_hyperedge(self,hyperedge):
        """
        Add a hyperedge.

        Params
        ------
        hyperedge (set): a set of nodes that have interaction
        with each other

        """
        self.hyperedges.add(frozenset(hyperedge))
        for node in hyperedge:
            self.add_node(node)

    def to_simplicial_complex(self):
        pass
        nodes = self.nodes
        simplices = self.hyperedges
        return SimplicialComplex(nodes=nodes,simplices=simplices)

    def to_graph(self):
        pass
        nodes = self.nodes
        edges = []
        for hyperedge in self.hyperedges:
            ## The following is redundant, we should not 
            ## have a 1 length hyperedge, but keeping
            ## it if it changes in the future
            if len(hyperedge) < 2:
                pass
                ## We are not keeping the point simplices
            elif len(hyperedge) == 2:
                edges.append(hyperedge)
            else:
                ## We are creating 2 length subsets to 
                ## the edge set
                edges.extend(get_subsets(hyperedge,2))
        return Graph(nodes=nodes,edges=edges)


if __name__ == '__main__':
    args = parser.parse_args()
    #  Wait for next request from client
    
    message = socket.recv()
    print("Received request from unity: %s" % message)
    #TypeError: a bytes-like object is required, not 'str'
    nodes_list, edges_list = parse_message(message.decode('utf8'))
    time.sleep(1)

    if args.graph_to_simplicial:
        graph = Graph(nodes_list, edges_list)
        graph_to_simplicial_complex = graph.to_simplicial_complex()
        print("graph_to_simplicial_complex",graph_to_simplicial_complex.simplices)
        socket.send(str(frozen_set_to_list(graph_to_simplicial_complex.simplices)).encode('ascii'))  
        
    if args.graph_to_hypergraph:
        graph = Graph(nodes_list, edges_list)
        graph_to_hypergraph = graph.to_hypergraph()
        print("graph_to_hypergraph",graph_to_hypergraph.hyperedges)
        socket.send(str(frozen_set_to_list(graph_to_hypergraph.hyperedges)).encode('ascii'))    
        
    if args.hypergraph_to_graph:
        hypergraph = HyperGraph(nodes_list, edges_list)
        hypergraph_to_graph = hypergraph.to_graph()
        print("hypergraph_to_graph",hypergraph_to_graph.edges)
        socket.send(str(frozen_set_to_list(hypergraph_to_graph.edges)).encode('ascii'))  
        
    if args.hypergraph_to_simplicial:
        hypergraph = HyperGraph(nodes_list, edges_list)
        hypergraph_to_simplicial = hypergraph.to_simplicial_complex()
        print("hypergraph_to_simplicial",hypergraph_to_simplicial.simplices)
        socket.send(str(frozen_set_to_list(hypergraph_to_simplicial.simplices)).encode('ascii'))  
        
    if args.simplicial_to_graph:
        simplicial = SimplicialComplex(nodes_list, edges_list)
        simplicial_to_graph = simplicial.to_graph()
        print("simplicial_to_graph",simplicial_to_graph.edges)
        socket.send(str(frozen_set_to_list(simplicial_to_graph.edges)).encode('ascii'))  
        
    if args.simplicial_to_hypergraph:
        simplicial = SimplicialComplex(nodes_list, edges_list)
        simplicial_to_hypergraph = simplicial.to_hypergraph()
        print("graph_from_hypergraph",simplicial_to_hypergraph.hyperedges)
        socket.send(str(frozen_set_to_list(simplicial_to_hypergraph.hyperedges)).encode('ascii'))  
    