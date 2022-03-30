using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Appy.Spatial.GeoJSON.Tests;

public class Serialisation
{
    readonly JsonSerializerOptions _options;
    readonly JsonSerializerSettings _settings;
    public Serialisation()
    {
        _options = new JsonSerializerOptions { Converters = { new TextJson.FeatureConverter(), new TextJson.GeometryConverter()}};
        _settings = new JsonSerializerSettings{ Converters = { new Newtonsoft.FeatureConverter(), new Newtonsoft.GeometryConverter()}};
    }
    [Fact]
    public void CanRoundTripPoint() =>
        test(new Point(new List<double> {1.0, 2.0}));
    
    [Fact]
    public void CanRoundTripLineString() =>
        test(new LineString(new List<IList<double>>
            {
                new List<double> {1.0, 2.0}
            }));
    
    [Fact]
    public void CanRoundTripMultiLineString() =>
        test(new MultiLineString(new List<IList<IList<double>>>
            {
                new List<IList<double>> {new List<double> {1.0, 2.0}}
            }));
    
    [Fact]
    public void CanRoundTripPolygon() =>
        test(new Polygon(new List<IList<IList<double>>>
            {
                new List<IList<double>> {new List<double> {1.0, 2.0}}
            }));
    
    [Fact]
    public void CanRoundTripMultiPolygon() =>
        test(new MultiPolygon(new List<IList<IList<IList<double>>>>
            {
                new List<IList<IList<double>>> { new List<IList<double>> {new List<double> {1.0, 2.0}}
            }}));
    
    [Fact]
    public void CanRoundTripGeometryCollection() =>
        test(new GeometryCollection 
            { 
                Geometries = new List<Geometry>{
                    new MultiPolygon(new List<IList<IList<IList<double>>>>
                    {
                        new List<IList<IList<double>>> { new List<IList<double>> {new List<double> {1.0, 2.0}}
                    }})}
            });

    void test<T>(T geometry) where T : Geometry
    {
        var textJsonOptions = new JsonSerializerOptions { Converters = {new TextJson.FeatureConverter(), new TextJson.GeometryConverter()}};
        var newtonsoftOptions = new JsonSerializerSettings{ Converters = { new Newtonsoft.FeatureConverter(), new Newtonsoft.GeometryConverter()}};
        // Test geometry
        Test(geometry, NewtonsoftSerialize, NewtonsoftDeserialise<Geometry>);
        Test(new Feature<T> {Geometry = geometry}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature>);
        Test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature<T, Props>>);
        Test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature<T, Props>>);
        Test(new NestedObject<T>(geometry), NewtonsoftSerialize, NewtonsoftDeserialise<NestedObject<T>>);
        
        Test(geometry, SystemTextSerialize, SystemTextDeserialise<Geometry>);
        Test(new Feature<T> {Geometry = geometry}, SystemTextSerialize, SystemTextDeserialise<Feature>);
        Test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, SystemTextSerialize, SystemTextDeserialise<Feature<T, Props>>);
        Test(new NestedObject<T>(geometry), SystemTextSerialize, SystemTextDeserialise<NestedObject<T>>);
    }

    void Test<TInput, TOutput>(TInput input, Func<object, string> serialiser, Func<string, TOutput> deserialiser)
    {
        var serialised = serialiser(input);
        var deserialised = deserialiser(serialised);
        deserialised.Should().BeOfType<TInput>();
        deserialised.Should().BeEquivalentTo(input);
    }
    
    string NewtonsoftSerialize(object obj) => JsonConvert.SerializeObject(obj, _settings);
    T NewtonsoftDeserialise<T>(string value) => JsonConvert.DeserializeObject<T>(value, _settings);
    
    string SystemTextSerialize(object obj) => System.Text.Json.JsonSerializer.Serialize(obj, _options);
    T SystemTextDeserialise<T>(string value) => System.Text.Json.JsonSerializer.Deserialize<T>(value, _options);
    
    public class Props
    {
        public string Id { get; set; }
    }
    
    public class NestedObject<T> where T : Geometry
    {
        public NestedObject(T geometry)
        {
            Geometry = geometry;
            Feature = new Feature<T>
            {
                Geometry = geometry
            };
            FeatureWithProps = new Feature<T, Props>
            {
                Geometry = Geometry,
                Properties = new Props {Id = "Test 2"}
            };
        }

        public Feature<T> Feature { get; set; }
        public Feature<T, Props> FeatureWithProps { get; set; }
        public T Geometry { get; set; }
    
    }

}