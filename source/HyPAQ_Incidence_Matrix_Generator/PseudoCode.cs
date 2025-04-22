using HypeToIncidenceMatrix.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace HyPAQ_Incidence_Matrix_Generator_v1
{
    internal class PseudoCode
    {
        /*
        BEGIN
    INPUT: Path to ".hype" file

    IF ".hype" file does not exist THEN
        PRINT "Error: File does not exist."
        EXIT
    END IF

    INITIALIZE empty list Vertices
    INITIALIZE empty list Hyperedges

    OPEN ".hype" file for reading

    SET currentSection = None

    FOR each line in ".hype" file DO
        TRIM whitespace from line

        IF line starts with "#" THEN
            IF line contains "Vertices" THEN
                SET currentSection = "Vertices"
            ELSE IF line contains "Hyperedges" THEN
                SET currentSection = "Hyperedges"
            ELSE IF line contains "Groups" THEN
                SET currentSection = "Groups"
            ELSE
                SET currentSection = None
            END IF
            CONTINUE
        END IF

        IF currentSection == "Vertices" THEN
        IF line is not empty THEN
                ADD line to Vertices list
            END IF
            CONTINUE
        END IF

        IF currentSection == "Hyperedges" THEN
            IF line is not empty THEN
                PARSE line into hyperedgeID and connectedVertices
                ADD tuple(hyperedgeID, connectedVertices) to Hyperedges list
            END IF
            CONTINUE
        END IF

        IF currentSection == "Groups" THEN
            IF line starts with "Group:" THEN
        SET currentGroup = extract group name
            ELSE IF line contains "Hyperedges:" THEN
                SET subSection = "GroupHyperedges"
            ELSE IF subSection == "GroupHyperedges" THEN
                IF line is not empty THEN
                    PARSE line into hyperedgeID and connectedVertices
                    ADD tuple(hyperedgeID, connectedVertices) to Hyperedges list
                END IF
            END IF
            CONTINUE
        END IF

        // Ignore Dependencies and other sections
    END FOR

    CLOSE ".hype" file

    ASSIGN row indices to Vertices(in order)
    ASSIGN column indices to Hyperedges(in order)

    INITIALIZE matrix M with dimensions[number of Vertices] x[number of Hyperedges], filled with 0

    FOR each hyperedge in Hyperedges DO
        SET columnIndex = index of hyperedge in Hyperedges list
        FOR each vertexID in hyperedge.connectedVertices DO
            IF vertexID exists in Vertices list THEN
                SET rowIndex = index of vertexID in Vertices list
                SET M[rowIndex][columnIndex] = 1
            ELSE
                PRINT "Warning: Vertex '{vertexID}' not found."
            END IF
        END FOR
    END FOR

    OPEN ".h" file for writing

    WRITE header row: tab-separated hyperedge IDs

    FOR each vertex in Vertices DO
        WRITE vertexID followed by tab-separated 0s and 1s indicating connections
    END FOR

    CLOSE ".h" file

    PRINT "Incidence matrix has been successfully written to '{output.h}'."
END

        */
    }
}
