using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Godot;
using Godot.Util;
using System;

public class LocationChunk : LayerChunk<LocationLayer, LocationChunk, LayerService> {

	struct Connection {
		public Location a;
		public Location b;
		public Connection(Location wa, Location wb) {
			a = wa;
			b = wb;
		}
	}

	Point[] initPositions;
	List<Location> locations;
	List<LocationSpec> locationSpecs = new List<LocationSpec>();
	List<Connection> connections = new List<Connection>();
	Color chunkColor;

	static RandomHash rand = new RandomHash(-1869717989);

	public LocationChunk() {
		initPositions = new Point[3];
		locations = new List<Location>(initPositions.Length);
	}

	// Used temporarily during generation.
	List<Point> allPositions = new List<Point>();

	public override void Create(int level, bool destroy, Action ready, Action done, LayerService service = null) {
		ready?.Invoke();
		float pushDist = bounds.size.x * 0.5f;
		int o = rand.GetInt(index.array);

		if (level == 0) {
			if (destroy) {

			}
			else {
				chunkColor = Color.FromHsv(Crd.Mod(index.x, 2) * 0.25f + Crd.Mod(index.y, 2) * 0.5f, 1f, 1f);

				// Create initial random positions of locations.
				for (int i = 0; i < initPositions.Length; i++) {
					initPositions[i] = new Point(
						 rand.Range(bounds.min.x, bounds.max.x, o + i * 100 + 1),
						 rand.Range(bounds.min.y, bounds.max.y, o + i * 100 + 2)
					);
				}
			}
		}

		if (level == 1) {
			if (destroy) {
				ObjectPool<Location>.GlobalReturnAll(locations);
				ObjectPool<LocationSpec>.GlobalReturnAll(locationSpecs);
			}
			else {
				// Adjust positions to be more evenly distributed.
				foreach (LocationChunk chunk in neighbors)
					allPositions.AddRange(chunk.initPositions);

				float maxPushLength = pushDist * 0.35f;
				// For each point in this chunk.
				for (int i = 0; i < initPositions.Length; i++) {
					// Calculate how each point from own chunk and neighbors
					// affect the current point.
					// Store result in Location further down, don't overwrite any data.
					Vector2 vect = Vector2.Zero;
					for (int j = 0; j < allPositions.Count; j++) {
						Vector2 dif = allPositions[j] - initPositions[i];
						if (dif == Vector2.Zero)
							continue;
						float dist = dif.Length();
						if (dist > pushDist)
							continue;
						Vector2 norm = dif / dist;
						vect -= norm * maxPushLength * (1 - dist / pushDist);
					}
					Point pos = initPositions[i]
					            + Point.GetRoundedPoint(vect.LimitLength(maxPushLength));

					// Create location from the adjusted position.
					Location location = ObjectPool<Location>.GlobalGet();
					locations.Add(location);
					location.position = pos;
					location.height = TerrainNoise.GetHeight(pos); //TODO: probably needs offset calculation it usually comes as -1:1
					location.radius = 10f;
					location.id = rand.GetInt(pos.x, pos.y);

					// Create location specification from the location.
					DPoint3 specPos = location.posWithHeight + DPoint3.up * 2;
					LocationSpec locationSpec = ObjectPool<LocationSpec>.GlobalGet()
						.Init((Vector3)specPos, 0, location.radius * 2, 20f, location.radius * 2 - 2, 0f);
					locationSpec.CalculateBounds();
					locationSpecs.Add(locationSpec);
				}
				allPositions.Clear();
			}
		}

		if (level == 2) {
			if (destroy) {
				connections.Clear();
			}
			else {
				CalculateConnectionsInOwnChunk();

				for (int axis = 0; axis <= 1; axis++)
					for (int rel = -1; rel <= 1; rel += 2)
						CalculateConnectionToChunk(axis, rel);

				foreach (Location loc in locations)
					loc.frontDir = loc.frontDir.Normalized();
			}
		}
		done?.Invoke();
	}

	public void GetConnectionsOwnedInBounds(List<Location> outConnectionPairs, GridBounds bounds) {
		foreach (var connection in connections) {
			Point a = connection.a.position;
			Point b = connection.b.position;
			Point center = (a + b) / 2;
			if (!bounds.Contains(center))
				continue;
			outConnectionPairs.Add(connection.a);
			outConnectionPairs.Add(connection.b);
		}
	}

	public void GetLocationsOwnedInBounds(List<Location> outLocations, GridBounds bounds) {
		foreach (var location in locations) {
			Point center = location.position;
			if (!bounds.Contains(center))
				continue;
			outLocations.Add(location);
		}
	}

	public void GetLocationSpecsOverlappingBounds(List<LocationSpec> outSpecs, GridBounds bounds) {
		foreach (var spec in locationSpecs) {
			if (!bounds.Overlaps(spec.bounds))
				continue;
			outSpecs.Add(spec);
		}
	}

	void CalculateConnectionsInOwnChunk() {
		float maxDist = 0;
		int maxDistIndex = -1;
		for (int i = 0; i < locations.Count; i++) {
			Location locationA = locations[i];
			Location locationB = locations[(i + 1) % locations.Count];
			connections.Add(new Connection(locationA, locationB));
			float dist = (locationA.position - locationB.position).sqrMagnitude;
			if (dist > maxDist) {
				maxDist = dist;
				maxDistIndex = i;
			}
		}

		if (connections.Count >= 3)
			connections.RemoveAt(maxDistIndex);

		foreach (Connection con in connections) {
			Vector2 dir = ((Vector2)(con.b.position - con.a.position)).Normalized();
			con.a.frontDir += dir;
			con.b.frontDir += -dir;
		}
	}

	void CalculateConnectionToChunk(int axis, int relative) {
		// Consider every pair of location from this chunk and neighboring one.
		Point vector = Point.zero;
		vector[axis] = relative;
		vector[1 - axis] = 0;
		LocationChunk otherChunk = layer.GetNeighborChunk(this, vector);
		Location a = null;
		Location b = null;
		float minDist = float.PositiveInfinity;
		foreach (Location locationA in locations) {
			foreach (Location locationB in otherChunk.locations) {
				float dist = (locationA.position - locationB.position).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					a = locationA;
					b = locationB;
				}
			}
		}

		Connection connection = new Connection(a, b);
		a.frontDir += ((Vector2)(b.position - a.position)).Normalized();
		if (bounds.Contains((a.position + b.position) / 2))
			connections.Add(connection);
	}

	public Location GetClosestLocation(Point p) {
		float distMin = float.PositiveInfinity;
		Location closest = null;
		foreach (LocationChunk chunk in neighbors) {
			foreach (Location w in chunk.locations) {
				Point vec = (w.position - p);
				float dist = ((Vector3)vec).Length() - w.radius;
				if (dist < distMin) {
					distMin = dist;
					closest = w;
				}
			}
		}
		return closest;
	}

	protected IEnumerable<LocationChunk> neighbors {
		get {
			return layer.GetNeighborChunks(this);
		}
	}

	public void DebugDraw() {
		DebugDraw(level);
	}

	public void DebugDraw(int level) {
		DebugDrawer.alpha = LocationLayer.debugInitial.animAlpha;
		if (level == 0 && DebugDrawer.alpha > 0) {
			foreach (Point p in initPositions)
				DebugDrawer.DrawCross(p, layer.chunkW * 0.05f, Colors.Gray);
		}
		DebugDrawer.alpha = LocationLayer.debugAdjusted.animAlpha;
		if (level == 1 && DebugDrawer.alpha > 0) {
			foreach (Point p in initPositions)
				DebugDrawer.DrawCross(p, layer.chunkW * 0.05f, Colors.Gray);
			for (int i = 0; i < locations.Count; i++) {
				DebugDrawer.DrawLine(locations[i].position.xoy, initPositions[i].xoy, Colors.Gray);
				DebugDrawer.DrawCross(locations[i].position, layer.chunkW * 0.05f, chunkColor);
			}
		}
		DebugDrawer.alpha = LocationLayer.debugRadiuses.animAlpha;
		if (level == 1 && DebugDrawer.alpha > 0) {
			for (int i = 0; i < locations.Count; i++)
				DebugDrawer.DrawCircle((Vector3)locations[i].posWithHeight, locations[i].radius, 16, chunkColor);
		}
		DebugDrawer.alpha = LocationLayer.debugConnections.animAlpha;
		if (level == 2 && DebugDrawer.alpha > 0) {
			for (int i = 0; i < locations.Count; i++)
				DebugDrawer.DrawCircle((Vector3)locations[i].posWithHeight, locations[i].radius, 16, chunkColor);
			foreach (Connection connection in connections) {
				Vector3 dir = ((Vector2)(connection.b.position - connection.a.position)).Normalized().xoy();
				Vector3 p1 = (Vector3)connection.a.posWithHeight + (dir * connection.a.radius);
				Vector3 p2 = (Vector3)connection.b.posWithHeight - (dir * connection.b.radius);
				DebugDrawer.DrawLine(p1, p2, chunkColor);
				DebugDrawer.DrawCross(p1, layer.chunkW * 0.01f, chunkColor);
			}
		}
		DebugDrawer.alpha = 1;
	}
}
