using System;
using System.Collections.Generic;
using System.Text.Json;
using Appy.Spatial.GeoJSON.Newtonsoft;
using Appy.Spatial.GeoJSON.TextJson;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Appy.Spatial.GeoJSON.Tests;

public class SerialisationTests
{
    readonly JsonSerializerOptions _textJsonSerializerOptions;
    readonly JsonSerializerSettings _newtonsoftSerializerSettings;
    
    public SerialisationTests()
    {
        _textJsonSerializerOptions = new JsonSerializerOptions().UseGeoJsonConverters();
        _newtonsoftSerializerSettings = new JsonSerializerSettings().UseGeoJsonConverters();
    }
    [Fact]
    public void ShouldRoundTripPoint() =>
        RoundTripTestCasesFor(new Point(new List<double> {1.0, 2.0}));
    
    [Fact]
    public void ShouldRoundTripLineString() =>
        RoundTripTestCasesFor(new LineString(new List<IList<double>>
            {
                new List<double> {1.0, 2.0}
            }));
    
    [Fact]
    public void ShouldRoundTripMultiLineString() =>
        RoundTripTestCasesFor(new MultiLineString(new List<IList<IList<double>>>
            {
                new List<IList<double>> {new List<double> {1.0, 2.0}}
            }));
    
    [Fact]
    public void ShouldRoundTripPolygon() =>
        RoundTripTestCasesFor(new Polygon(new List<IList<IList<double>>>
            {
                new List<IList<double>> {new List<double> {1.0, 2.0}}
            }));
    
    [Fact]
    public void ShouldRoundTripMultiPolygon() =>
        RoundTripTestCasesFor(new MultiPolygon(new List<IList<IList<IList<double>>>>
            {
                new List<IList<IList<double>>> { new List<IList<double>> {new List<double> {1.0, 2.0}}
            }}));
    
    [Fact]
    public void ShouldRoundTripGeometryCollection() =>
        RoundTripTestCasesFor(new GeometryCollection 
            { 
                Geometries = new List<Geometry>{
                    new MultiPolygon(new List<IList<IList<IList<double>>>>
                    {
                        new List<IList<IList<double>>> { new List<IList<double>> {new List<double> {1.0, 2.0}}
                    }})}
            });

    void RoundTripTestCasesFor<T>(T geometry) where T : Geometry
    {
        ShouldSerializeAndDeserialize(geometry, NewtonsoftSerialize, NewtonsoftDeserialise<Geometry>);
        ShouldSerializeAndDeserialize(new Feature<T> {Geometry = geometry}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature>);
        ShouldSerializeAndDeserialize(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature<T, Props>>);
        ShouldSerializeAndDeserialize(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, NewtonsoftSerialize, NewtonsoftDeserialise<Feature<T, Props>>);
        ShouldSerializeAndDeserialize(new NestedObject<T>(geometry), NewtonsoftSerialize, NewtonsoftDeserialise<NestedObject<T>>);
        
        ShouldSerializeAndDeserialize(geometry, SystemTextSerialize, SystemTextDeserialize<Geometry>);
        ShouldSerializeAndDeserialize(new Feature<T> {Geometry = geometry}, SystemTextSerialize, SystemTextDeserialize<Feature>);
        ShouldSerializeAndDeserialize(new Feature<T, Props> {Geometry = geometry, Properties = new Props{ Id = "test"}}, SystemTextSerialize, SystemTextDeserialize<Feature<T, Props>>);
        ShouldSerializeAndDeserialize(new NestedObject<T>(geometry), SystemTextSerialize, SystemTextDeserialize<NestedObject<T>>);
    }

    static void ShouldSerializeAndDeserialize<TInput, TOutput>(TInput input, Func<object, string> serializer, Func<string, TOutput> deserializer)
    {
        var serialised = serializer(input);
        var deserialized = deserializer(serialised);
        deserialized.Should().BeOfType<TInput>();
        deserialized.Should().BeEquivalentTo(input);
    }
    
    string NewtonsoftSerialize(object obj) => JsonConvert.SerializeObject(obj, _newtonsoftSerializerSettings);
    T NewtonsoftDeserialise<T>(string value) => JsonConvert.DeserializeObject<T>(value, _newtonsoftSerializerSettings);
    
    string SystemTextSerialize(object obj) => System.Text.Json.JsonSerializer.Serialize(obj, _textJsonSerializerOptions);
    T SystemTextDeserialize<T>(string value) => System.Text.Json.JsonSerializer.Deserialize<T>(value, _textJsonSerializerOptions);
    
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