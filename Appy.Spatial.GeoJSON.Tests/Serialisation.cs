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
        _options = new JsonSerializerOptions { Converters = {new TextJson.FeatureConverter(), new TextJson.GeometryConverter()}};
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
        test(geometry, nsSerialize, nsDeserialise<Geometry>);
        test(new Feature<T> {Geometry = geometry}, nsSerialize, nsDeserialise<Feature>);
        test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, nsSerialize, nsDeserialise<Feature<T, Props>>);
        test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, nsSerialize, nsDeserialise<Feature<T, Props>>);
        test(new NestedObject<T>(geometry), nsSerialize, nsDeserialise<NestedObject<T>>);
        
        test(geometry, textSerialize, textDeserialise<Geometry>);
        test(new Feature<T> {Geometry = geometry}, textSerialize, textDeserialise<Feature>);
        test(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, textSerialize, textDeserialise<Feature<T, Props>>);
        test(new NestedObject<T>(geometry), textSerialize, textDeserialise<NestedObject<T>>);
    }

    void test<TInput, TOutput>(TInput input, Func<object, string> serialiser, Func<string, TOutput> deserialiser)
    {
        var serialised = serialiser(input);
        var deserialised = deserialiser(serialised);
        deserialised.Should().BeOfType<TInput>();
        deserialised.Should().BeEquivalentTo(input);
    }
    
    string nsSerialize(object obj) => JsonConvert.SerializeObject(obj, _settings);
    T nsDeserialise<T>(string value) => JsonConvert.DeserializeObject<T>(value, _settings);
    
    string textSerialize(object obj) => System.Text.Json.JsonSerializer.Serialize(obj, _options);
    T textDeserialise<T>(string value) => System.Text.Json.JsonSerializer.Deserialize<T>(value, _options);
    
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