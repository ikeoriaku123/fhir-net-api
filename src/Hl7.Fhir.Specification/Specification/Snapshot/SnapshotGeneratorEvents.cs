﻿/* 
 * Copyright (c) 2016, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using Hl7.Fhir.Model;

namespace Hl7.Fhir.Specification.Snapshot
{
    // Event declarations for the SnapshotGenerator class

    public partial class SnapshotGenerator
    {
        /// <summary>
        /// An event that notifies clients when a differential constraint has been processed.
        /// The specified <see cref="Element"/> instance is the result of merging the base
        /// element with the associated differential constraint(s).
        /// The event handler can inspect and optionally modify the element.
        /// The result will be included in the snapshot.
        /// </summary>
        public event SnapshotConstraintHandler Constraint;

        /// <summary>Raise the <see cref="Constraint"/> event to notify the client that a differential constraint has been processed.</summary>
        /// <param name="element">A reference to the snapshot element with merged differential constraints.</param>
        internal void OnConstraint(Element element)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            
            // Configurable default behavior: mark changed elements
            if (_settings.MarkChanges)
            {
                element.SetExtension(CHANGED_BY_DIFF_EXT, new FhirBoolean(true));
            }

            var handler = Constraint;
            if (handler != null)
            {
                var args = new SnapshotConstraintEventArgs(element);
                handler(this, args);
            }
        }

        /// <summary>
        /// An event that notifies clients when a base profile has been resolved.
        /// </summary>
        /// <remarks>
        /// The <see cref="SnapshotBaseProfileEventArgs.BaseProfile"/> event argument returns a
        /// reference to the original base profile instance as returned by the artifact source.
        /// Modifications to this instance will affect the original cached artifact
        /// and will be visible to other consumers of the artifact source.
        /// </remarks>
        public event SnapshotBaseProfileHandler PrepareBaseProfile;

        /// <summary>
        /// Raise the <see cref="PrepareBaseProfile"/> event to notify the client
        /// when the associated base profile has been resolved and prepared for merging.
        /// </summary>
        /// <param name="profile">A profile <see cref="StructureDefinition"/> instance.</param>
        /// <param name="baseProfile">The associated base profile <see cref="StructureDefinition"/> instance.</param>
        internal void OnPrepareBaseProfile(StructureDefinition profile, StructureDefinition baseProfile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }
            if (baseProfile == null) { throw new ArgumentNullException("baseProfile"); }
            var handler = PrepareBaseProfile;
            if (handler != null)
            {
                var args = new SnapshotBaseProfileEventArgs(profile, baseProfile);
                handler(this, args);
            }
        }

        /// <summary>
        /// An event that notifies clients when the generator initializes a new snapshot element.
        /// The specified element is cloned from the base profile and the base path has been fixed.
        /// The event handler can inspect and optionally modify the base element clone.
        /// After the event handler returns, the snapshot generator merges any associated
        /// differential constraints and the result is included in the snapshot.
        /// </summary>
        public event SnapshotElementHandler PrepareBaseElement;

        /// <summary>Raise the <see cref="PrepareBaseElement"/> event to notify the client when a base element definition is being prepared for merging.</summary>
        internal void OnPrepareBaseElement(ElementDefinition element)
        {
            if (element == null) { throw new ArgumentNullException("element"); }
            var handler = PrepareBaseElement;
            if (handler != null)
            {
                var args = new SnapshotElementEventArgs(element);
                handler(this, args);
            }
        }
    }

    /// <summary>Event arguments for the <see cref="SnapshotConstraintHandler"/> event delegate.</summary>
    public class SnapshotConstraintEventArgs : EventArgs
    {
        private readonly Element _element;

        public SnapshotConstraintEventArgs(Element element) : base()
        {
            _element = element;
        }

        /// <summary>Returns a reference to a constrained snapshot element definition or property.</summary>
        public Element Element { get { return _element; } }
    }

    /// <summary>A delegate type for hooking up <see cref="SnapshotGenerator.Constraint"/> events.</summary>
    public delegate void SnapshotConstraintHandler(object sender, SnapshotConstraintEventArgs e);


    /// <summary>Event arguments for the <see cref="SnapshotBaseProfileHandler"/> event delegate.</summary>
    public class SnapshotBaseProfileEventArgs : EventArgs
    {
        private readonly StructureDefinition _profile;
        private readonly StructureDefinition _baseProfile;

        public SnapshotBaseProfileEventArgs(StructureDefinition profile, StructureDefinition baseProfile) : base()
        {
            _profile = profile;
            _baseProfile = baseProfile;
        }

        /// <summary>Returns a reference to a profile.</summary>
        public StructureDefinition Profile { get { return _profile; } }

        /// <summary>Returns a reference to the associated base profile.</summary>
        public StructureDefinition BaseProfile { get { return _baseProfile; } }

    }

    /// <summary>A delegate type for hooking up <see cref="SnapshotGenerator.PrepareBaseProfile"/> events.</summary>
    public delegate void SnapshotBaseProfileHandler(object sender, SnapshotBaseProfileEventArgs e);


    /// <summary>Event arguments for the <see cref="SnapshotElementHandler"/> event delegate.</summary>
    public class SnapshotElementEventArgs : EventArgs
    {
        private readonly ElementDefinition _element;

        public SnapshotElementEventArgs(ElementDefinition element) : base()
        {
            _element = element;
        }

        /// <summary>Returns a reference to an element definition.</summary>
        public ElementDefinition Element { get { return _element; } }
    }

    public delegate void SnapshotElementHandler(object sender, SnapshotElementEventArgs e);

}